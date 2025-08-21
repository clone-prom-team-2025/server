using System.Diagnostics;
using System.Runtime.InteropServices;
using App.Core.Interfaces;
using App.Core.Models.FileStorage;
using Microsoft.Extensions.Options;
using MyApp.Core.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Xabe.FFmpeg;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using MongoDB.Bson;

namespace App.Services;

public class FileService : IFileService
{
    private readonly FileStorageOptions _options;
    private readonly MinIOOptions _minIOoptions;
    private readonly IAmazonS3 _s3Client;
    private readonly int fileUniqueidLength = 10;

    private readonly TransferUtility _transferUtility;

    public FileService(IOptions<FileStorageOptions> options, IOptions<MinIOOptions> minioOptions)
    {
        _options = options.Value;
        _minIOoptions = minioOptions.Value;

        var credentials = new BasicAWSCredentials(_minIOoptions.AccessKey, _minIOoptions.SecretKey);

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _minIOoptions.Endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = "auto",
        };

        _s3Client = new AmazonS3Client(credentials, s3Config);
        _transferUtility = new TransferUtility(_s3Client);

        var ffmpegPath = GetFfmpegPath(_options.FfmpegPath);
        FFmpeg.SetExecutablesPath(Path.GetDirectoryName(ffmpegPath)!);
    }

    private string GetFfmpegPath(string? userDefinedPath)
    {
        if (!string.IsNullOrWhiteSpace(userDefinedPath) && File.Exists(userDefinedPath))
            return userDefinedPath;

        string ffmpegName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which",
                Arguments = ffmpegName,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            process!.WaitForExit();

            var result = process.StandardOutput.ReadLine();
            if (!string.IsNullOrWhiteSpace(result) && File.Exists(result))
                return result;
        }
        catch { }

        throw new FileNotFoundException("FFmpeg executable not found. Please install FFmpeg or set path in appsettings.json.");
    }

    public string GetPreSignedURL(string bucketName, string objectKey, double expirationMinutes = 60)
    {
        var request = new Amazon.S3.Model.GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
        return _s3Client.GetPreSignedURL(request);
    }


    private string GenerateFileName(string originalName, string suffix, string extension)
    {
        var id = NanoIdGenerator.Generate(fileUniqueidLength);
        var sanitized = Path.GetFileNameWithoutExtension(originalName).Trim().Replace(" ", "-");
        return $"{id}_{sanitized}{suffix}{extension}";
    }

    private string GenerateFileName(string id, string originalName, string suffix, string extension)
    {
        var sanitized = Path.GetFileNameWithoutExtension(originalName).Trim().Replace(" ", "-");
        return $"{id}_{sanitized}{suffix}{extension}";
    }

    private async Task<string> UploadAsync(string keyPrefix, Stream fileStream, string fileName)
    {
        var key = $"{keyPrefix.TrimEnd('/')}/{fileName}";
        var request = new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = _minIOoptions.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = GetMimeType(fileName),
        };
        await _s3Client.PutObjectAsync(request);
        return $"{_minIOoptions.PublicBaseUrl.TrimEnd('/')}/{key}";
    }

    private string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".webp" => "image/webp",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webm" => "video/webm",
            ".mp4" => "video/mp4",
            _ => "application/octet-stream"
        };
    }

    public async Task<(string SourceUrl, string CompressedUrl, string SourceName, string CompressedFileName)> SaveImageAsync(Stream imageStream, string fileName, string key)
    {
        var id = NanoIdGenerator.Generate(fileUniqueidLength);
        var baseFileName = Path.GetFileNameWithoutExtension(fileName);

        var sourceName = GenerateFileName(id, baseFileName, "_source", ".webp");
        var compressedName = GenerateFileName(id, baseFileName, "_compressed", ".webp");

        var tempInput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(fileName)}");
        var tempSource = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_source.webp");
        var tempCompressed = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_compressed.webp");

        try
        {
            await using (var fs = File.Create(tempInput))
                await imageStream.CopyToAsync(fs);
            
            var sourceConversion = FFmpeg.Conversions.New()
                .AddParameter($"-i \"{tempInput}\"", ParameterPosition.PreInput)
                .AddParameter("-vf \"scale=w=1280:h=720:force_original_aspect_ratio=decrease\"")
                .AddParameter("-c:v libwebp -preset picture -compression_level 6 -quality 90 -threads 0")
                .SetOutput(tempSource);


            var compressedConversion = FFmpeg.Conversions.New()
                .AddParameter($"-i \"{tempInput}\"", ParameterPosition.PreInput)
                .AddParameter("-vf \"scale=w=1280:h=720:force_original_aspect_ratio=decrease\"")
                .AddParameter("-c:v libwebp -preset picture -compression_level 6 -quality 60 -threads 0")
                .SetOutput(tempCompressed);

            var sourceTask = sourceConversion.Start();
            var compressedTask = compressedConversion.Start();
            await Task.WhenAll(sourceTask, compressedTask);

            await using var sourceStream = File.OpenRead(tempSource);
            await using var compressedStream = File.OpenRead(tempCompressed);

            var uploadSource = UploadAsync(key, sourceStream, sourceName);
            var uploadCompressed = UploadAsync(key, compressedStream, compressedName);

            await Task.WhenAll(uploadSource, uploadCompressed);

            return (uploadSource.Result, uploadCompressed.Result, sourceName, compressedName);
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempSource)) File.Delete(tempSource);
            if (File.Exists(tempCompressed)) File.Delete(tempCompressed);
        }
    }

    public async Task<(string Url, string FileName)> SaveVideoAsync(Stream videoStream, string fileName, string key)
    {
        var id = NanoIdGenerator.Generate(fileUniqueidLength);
        var baseFileName = Path.GetFileNameWithoutExtension(videoStream.Length > 0 ? fileName : "video");
        var outputWebmFileName = GenerateFileName(id, baseFileName, "", ".webm");

        var tempDir = Path.Combine("wwwroot", "temp");
        Directory.CreateDirectory(tempDir);

        var inputPath = Path.Combine(tempDir, Guid.NewGuid() + Path.GetExtension(fileName));
        var outputWebmPath = Path.Combine(tempDir, Guid.NewGuid() + ".webm");

        // Зберігаємо вхідний стрім у файл
        await using (var fileStream = File.Create(inputPath))
            await videoStream.CopyToAsync(fileStream);

        // Конвертація у WebM
        var conversionWebm = FFmpeg.Conversions.New()
            .AddParameter($"-i \"{inputPath}\"", ParameterPosition.PreInput)
            .AddParameter("-vf scale='min(1280,iw)':'min(720,ih)'")
            .AddParameter("-c:v libvpx -b:v 2M")
            .AddParameter("-c:a libvorbis -b:a 128k")
            .SetOutput(outputWebmPath);

        await conversionWebm.Start();

        // Видаляємо вхідний файл
        File.Delete(inputPath);

        // Завантажуємо WebM на S3/MinIO
        await using var webmStream = File.OpenRead(outputWebmPath);
        var url = await UploadAsync(key, webmStream, outputWebmFileName);

        // Очищаємо тимчасовий WebM файл
        File.Delete(outputWebmPath);

        return (url, outputWebmFileName);
    }

    public async Task DeleteFileAsync(string key, string fileName)
    {
        string Key = $"{key}/{fileName}";
        var result = await _s3Client.DeleteObjectAsync(_minIOoptions.BucketName, Key);
    }
}
