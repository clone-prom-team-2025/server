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
    private readonly CloudflareR2Options _r2Options;
    private readonly IAmazonS3 _s3Client;
    private readonly int fileUniqueidLength = 10;

    private readonly TransferUtility _transferUtility;

    public FileService(IOptions<FileStorageOptions> options, IOptions<CloudflareR2Options> r2Options)
    {
        _options = options.Value;
        _r2Options = r2Options.Value;

        var credentials = new BasicAWSCredentials(_r2Options.AccessKey, _r2Options.SecretKey);

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _r2Options.Endpoint,
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
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            using (var tempFile = File.Create(tempPath))
            {
                await fileStream.CopyToAsync(tempFile);
            }

            using (var fs = File.OpenRead(tempPath))
            {
                var r2Key = $"{keyPrefix.TrimEnd('/')}/{fileName}";
                var request = new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = _r2Options.BucketName,
                    Key = r2Key,
                    InputStream = fs,
                    //DisablePayloadSigning = true,
                    ContentType = GetMimeType(fileName),
                };
                await _s3Client.PutObjectAsync(request);

                // Використовуємо PublicBaseUrl як корінь для публічних URL
                return $"{_r2Options.PublicBaseUrl.TrimEnd('/')}/{r2Key}";
            }
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
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

    public async Task<(string FullHdUrl, string HdUrl, string UrlFileName, string SecondUrlFileName)> SaveImageFullHdAndHdAsync(Stream imageStream, string fileName, string key)
    {
        using var image = await Image.LoadAsync(imageStream);

        var fullHdSize = new Size(Math.Min(1920, image.Width), Math.Min(1080, image.Height));
        var hdSize = new Size(1280, 720);

        var baseFileName = Path.GetFileNameWithoutExtension(fileName);
        var id = NanoIdGenerator.Generate(fileUniqueidLength);

        var fullHdName = GenerateFileName(id, baseFileName, "_fullhd", ".webp");
        var hdName = GenerateFileName(id, baseFileName, "_hd", ".webp");

        var tempDir = Path.Combine("wwwroot", "temp");
        Directory.CreateDirectory(tempDir);

        var fullHdPath = Path.Combine(tempDir, fullHdName);
        var hdPath = Path.Combine(tempDir, hdName);

        image.Mutate(x => x.Resize(new ResizeOptions { Size = fullHdSize, Mode = ResizeMode.Max }));
        await image.SaveAsync(fullHdPath, new WebpEncoder());

        image.Mutate(x => x.Resize(new ResizeOptions { Size = hdSize, Mode = ResizeMode.Max }));
        await image.SaveAsync(hdPath, new WebpEncoder());

        await using var fullHdFileStream = File.OpenRead(fullHdPath);
        var fullHdUrl = await UploadAsync(key, fullHdFileStream, fullHdName);

        await using var hdFileStream = File.OpenRead(hdPath);
        var hdUrl = await UploadAsync(key, hdFileStream, hdName);

        File.Delete(fullHdPath);
        File.Delete(hdPath);

        return (fullHdUrl, hdUrl, fullHdName, hdName);
    }
    
    public async Task<(string FullHdUrl, string UrlFileName)> SaveImageFullHdAsync(Stream imageStream, string fileName, string key)
    {
        using var image = await Image.LoadAsync(imageStream);

        var fullHdSize = new Size(Math.Min(1920, image.Width), Math.Min(1080, image.Height));
        
        var baseFileName = Path.GetFileNameWithoutExtension(fileName);
        var id = NanoIdGenerator.Generate(fileUniqueidLength);

        var fullHdName = GenerateFileName(id, baseFileName, "_fullhd", ".webp");

        var tempDir = Path.Combine("wwwroot", "temp");
        Directory.CreateDirectory(tempDir);

        var fullHdPath = Path.Combine(tempDir, fullHdName);

        image.Mutate(x => x.Resize(new ResizeOptions { Size = fullHdSize, Mode = ResizeMode.Max }));
        await image.SaveAsync(fullHdPath, new WebpEncoder());

        await using var fullHdFileStream = File.OpenRead(fullHdPath);
        var fullHdUrl = await UploadAsync(key, fullHdFileStream, fullHdName);
        

        File.Delete(fullHdPath);

        return (fullHdUrl, fullHdName);
    }

    public async Task<(string Url, string FileName)> SaveVideoAsync(Stream videoStream, string fileName, string key)
    {
        var id = NanoIdGenerator.Generate(fileUniqueidLength);
        var baseFileName = Path.GetFileNameWithoutExtension(fileName);
        var outputFileName = GenerateFileName(id, baseFileName, "", ".webm");

        var tempDir = Path.Combine("wwwroot", "temp");
        Directory.CreateDirectory(tempDir);

        var inputPath = Path.Combine(tempDir, Guid.NewGuid() + Path.GetExtension(fileName));
        var outputPath = Path.Combine(tempDir, Guid.NewGuid() + ".webm");

        using (var fileStream = File.Create(inputPath))
        {
            await videoStream.CopyToAsync(fileStream);
        }

        var conversion = FFmpeg.Conversions.New()
            .AddParameter($"-i \"{inputPath}\"", ParameterPosition.PreInput)
            .AddParameter("-vf scale='min(1280,iw)':'min(720,ih)'")
            .SetOutput(outputPath);

        await conversion.Start();

        File.Delete(inputPath);

        await using var outputStream = File.OpenRead(outputPath);
        var url = await UploadAsync(key, outputStream, outputFileName);

        File.Delete(outputPath);

        return (url, outputFileName);
    }

    public async Task DeleteFileAsync(string key, string fileName)
    {
        string Key = $"{key}/{fileName}";
        //Console.WriteLine(r2Key);
        var result = await _s3Client.DeleteObjectAsync(_r2Options.BucketName, Key);
    }
}
