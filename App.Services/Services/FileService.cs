using System.Diagnostics;
using System.Runtime.InteropServices;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using App.Core.Interfaces;
using App.Core.Models.FileStorage;
using Microsoft.Extensions.Options;
using MyApp.Core.Utils;
using Xabe.FFmpeg;

namespace App.Services.Services;

/// <summary>
/// Handles file storage operations including image and video conversion,
/// uploading to S3/MinIO, and file deletion.
/// </summary>
public class FileService : IFileService
{
    private readonly MinIOOptions _minIOptions;
    private readonly IAmazonS3 _s3Client;

    private readonly int _fileUniqueLength = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// Sets up S3 client and FFmpeg path.
    /// </summary>
    public FileService(IOptions<FileStorageOptions> options, IOptions<MinIOOptions> minioOptions)
    {
        var options1 = options.Value;
        _minIOptions = minioOptions.Value;

        var credentials = new BasicAWSCredentials(_minIOptions.AccessKey, _minIOptions.SecretKey);

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _minIOptions.Endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = "auto"
        };

        _s3Client = new AmazonS3Client(credentials, s3Config);

        var ffmpegPath = GetFfmpegPath(options1.FfmpegPath);
        FFmpeg.SetExecutablesPath(Path.GetDirectoryName(ffmpegPath)!);
    }

    /// <summary>
    /// Converts an input image to WebP format, generates a compressed version,
    /// uploads both files to storage, and provides their URLs and names.
    /// </summary>
    public async Task<(string SourceUrl, string CompressedUrl, string SourceName, string CompressedFileName)>
        SaveImageAsync(Stream imageStream, string fileName, string key)
    {
        var id = NanoIdGenerator.Generate(_fileUniqueLength);
        var baseFileName = Path.GetFileNameWithoutExtension(fileName);

        var sourceName = GenerateFileName(id, baseFileName, "_source", ".webp");
        var compressedName = GenerateFileName(id, baseFileName, "_compressed", ".webp");

        var tempInput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(fileName)}");
        var tempSource = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_source.webp");
        var tempCompressed = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_compressed.webp");

        try
        {
            await using (var fs = File.Create(tempInput))
            {
                await imageStream.CopyToAsync(fs);
            }

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

    /// <summary>
    /// Converts an input video to WebM format, uploads it to storage, and provides the URL and filename.
    /// </summary>
    public async Task<(string Url, string FileName)> SaveVideoAsync(Stream videoStream, string fileName, string key)
    {
        var id = NanoIdGenerator.Generate(_fileUniqueLength);
        var baseFileName = Path.GetFileNameWithoutExtension(videoStream.Length > 0 ? fileName : "video");
        var outputWebmFileName = GenerateFileName(id, baseFileName, "", ".webm");

        var tempDir = Path.Combine("wwwroot", "temp");
        Directory.CreateDirectory(tempDir);

        var inputPath = Path.Combine(tempDir, Guid.NewGuid() + Path.GetExtension(fileName));
        var outputWebmPath = Path.Combine(tempDir, Guid.NewGuid() + ".webm");

        // Зберігаємо вхідний стрім у файл
        await using (var fileStream = File.Create(inputPath))
        {
            await videoStream.CopyToAsync(fileStream);
        }

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

    /// <summary>
    /// Deletes a file from S3/MinIO storage.
    /// </summary>
    public async Task DeleteFileAsync(string key, string fileName)
    {
        var key1 = $"{key}/{fileName}";
        await _s3Client.DeleteObjectAsync(_minIOptions.BucketName, key1);
    }

    /// <summary>
    /// Finds the path of FFmpeg executable based on user-defined path or system environment.
    /// Throws FileNotFoundException if FFmpeg cannot be found.
    /// </summary>
    private string GetFfmpegPath(string? userDefinedPath)
    {
        if (!string.IsNullOrWhiteSpace(userDefinedPath) && File.Exists(userDefinedPath))
            return userDefinedPath;

        var ffmpegName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";

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
        catch (Exception ex)
        {
            throw new Exception("Unable to get ffmpeg path", ex);
        }

        throw new FileNotFoundException(
            "FFmpeg executable not found. Please install FFmpeg or set path in appsettings.json.");
    }

    /// <summary>
    /// Generates a pre-signed URL for accessing a file with a limited expiration time.
    /// </summary>
    public string GetPreSignedUrl(string bucketName, string objectKey, double expirationMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
        return _s3Client.GetPreSignedURL(request);
    }

    // /// <summary>
    // /// Generates a unique filename for storage.
    // /// </summary>
    // private string GenerateFileName(string originalName, string suffix, string extension)
    // {
    //     var id = NanoIdGenerator.Generate(_fileUniqueLength);
    //     var sanitized = Path.GetFileNameWithoutExtension(originalName).Trim().Replace(" ", "-");
    //     return $"{id}_{sanitized}{suffix}{extension}";
    // }

    /// <summary>
    /// Generates a unique filename using a provided ID for storage.
    /// </summary>
    private string GenerateFileName(string id, string originalName, string suffix, string extension)
    {
        var sanitized = Path.GetFileNameWithoutExtension(originalName).Trim().Replace(" ", "-");
        return $"{id}_{sanitized}{suffix}{extension}";
    }

    /// <summary>
    /// Uploads a file stream to S3/MinIO storage and returns the public URL.
    /// </summary>
    private async Task<string> UploadAsync(string keyPrefix, Stream fileStream, string fileName)
    {
        var key = $"{keyPrefix.TrimEnd('/')}/{fileName}";
        var request = new PutObjectRequest
        {
            BucketName = _minIOptions.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = GetMimeType(fileName)
        };
        await _s3Client.PutObjectAsync(request);
        return $"{_minIOptions.PublicBaseUrl.TrimEnd('/')}/{key}";
    }

    /// <summary>
    /// Determines the MIME type of file based on its extension.
    /// </summary>
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
}