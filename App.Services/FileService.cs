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

namespace App.Services;

/// <summary>
/// Provides functionality to process and store image and video files
/// including format conversion, resizing, and structured storage.
/// </summary>
public class FileService : IFileService
{
    private readonly FileStorageOptions _options;

    private readonly int fileUniqueidLenght = 10;

    /// <summary>
    /// Initializes the <see cref="FileService"/> instance and configures FFmpeg.
    /// </summary>
    /// <param name="options">Injected file storage configuration options.</param>
    /// <exception cref="FileNotFoundException">Thrown if FFmpeg is not found.</exception>
    public FileService(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;

        var ffmpegPath = GetFfmpegPath(_options.FfmpegPath);
        FFmpeg.SetExecutablesPath(Path.GetDirectoryName(ffmpegPath)!);
    }

    /// <summary>
    /// Detects the FFmpeg binary location. Attempts to resolve automatically
    /// via environment or uses explicitly defined path.
    /// </summary>
    /// <param name="userDefinedPath">Optional manual path to ffmpeg binary.</param>
    /// <returns>Full path to ffmpeg binary.</returns>
    /// <exception cref="FileNotFoundException">If ffmpeg could not be found.</exception>
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

    /// <summary>
    /// Generates a sanitized, unique file name based on the original name and suffix.
    /// </summary>
    /// <param name="originalName">Original file name.</param>
    /// <param name="suffix">Suffix to append (e.g., "_fullhd").</param>
    /// <param name="extension">Target file extension (e.g., ".webp").</param>
    /// <returns>Sanitized unique file name.</returns>
    private string GenerateFileName(string originalName, string suffix, string extension)
    {
        var id = NanoIdGenerator.Generate(fileUniqueidLenght);
        var sanitized = Path.GetFileNameWithoutExtension(originalName)
                            .Trim()
                            .Replace(" ", "-");

        return $"{id}_{sanitized}{suffix}{extension}";
    }

    /// <summary>
    /// Generates a sanitized, unique file name using a provided identifier.
    /// </summary>
    /// <param name="id">Unique ID to prefix.</param>
    /// <param name="originalName">Original file name.</param>
    /// <param name="suffix">Suffix to append (e.g., "_hd").</param>
    /// <param name="extension">Target file extension (e.g., ".webp").</param>
    /// <returns>Sanitized unique file name.</returns>
    private string GenerateFileName(string id, string originalName, string suffix, string extension)
    {
        var sanitized = Path.GetFileNameWithoutExtension(originalName)
                            .Trim()
                            .Replace(" ", "-");

        return $"{id}_{sanitized}{suffix}{extension}";
    }

    /// <summary>
    /// Saves an image in both Full HD (max 1920x1080) and HD (1280x720) resolutions.
    /// Output format is WebP.
    /// </summary>
    /// <param name="imageStream">Input image stream.</param>
    /// <param name="fileName">Original file name for reference.</param>
    /// <returns>Tuple of saved file paths: (FullHD, HD).</returns>
    public async Task<(string FullHdPath, string HdPath, string fileName)> SaveImageAsync(Stream imageStream, string fileName)
    {
        using var image = await Image.LoadAsync(imageStream);

        var fullHdSize = new Size(Math.Min(1920, image.Width), Math.Min(1080, image.Height));
        var hdSize = new Size(1280, 720);

        Directory.CreateDirectory(_options.FullHdImagePath);
        Directory.CreateDirectory(_options.HdImagePath);

        var baseFileName = Path.GetFileNameWithoutExtension(fileName);

        var id = NanoIdGenerator.Generate(fileUniqueidLenght);
        var fullHdName = GenerateFileName(id, baseFileName, "_fullhd", ".webp");
        var hdName = GenerateFileName(id, baseFileName, "_hd", ".webp");

        string fullHdFile = Path.Combine(_options.FullHdImagePath, fullHdName);
        string hdFile = Path.Combine(_options.HdImagePath, hdName);

        // Full HD
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = fullHdSize,
            Mode = ResizeMode.Max
        }));
        await image.SaveAsync(fullHdFile, new WebpEncoder());

        // HD
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = hdSize,
            Mode = ResizeMode.Max
        }));
        await image.SaveAsync(hdFile, new WebpEncoder());

        return (fullHdFile, hdFile, GenerateFileName(id, baseFileName, "", ".webp"));
    }

    /// <summary>
    /// Saves a video file by converting it to WebM format and scaling it to max 1280x720.
    /// </summary>
    /// <param name="videoStream">Input video stream.</param>
    /// <param name="fileName">Original file name for reference.</param>
    /// <returns>Path to the saved WebM file.</returns>
    public async Task<(string Url, string FileName)> SaveVideoAsync(Stream videoStream, string fileName)
    {
        var id = NanoIdGenerator.Generate(fileUniqueidLenght);
        var baseFileName = Path.GetFileNameWithoutExtension(fileName);

        var outputFileName = $"{id}_{baseFileName}.webm";

        var inputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(fileName));
        var outputPath = Path.Combine(_options.VideoPath, outputFileName);

        Directory.CreateDirectory(_options.VideoPath);

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
        return (outputPath, GenerateFileName(id, baseFileName, "", ".webm"));
    }

    /// <summary>
    /// Deletes the specified file if it exists.
    /// </summary>
    /// <param name="path">Absolute file path to delete.</param>
    public void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
