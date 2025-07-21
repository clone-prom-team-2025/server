using System.Diagnostics;
using System.Runtime.InteropServices;
using App.Core.Interfaces;
using App.Core.Models.FileStorage;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Xabe.FFmpeg;

namespace App.Services;

public class FileService : IFileService
{
    private readonly FileStorageOptions _options;

    public FileService(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;

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

    public async Task<(string FullHdPath, string HdPath)> SaveImageAsync(Stream imageStream, string fileName)
    {
        using var image = await Image.LoadAsync(imageStream);

        var fullHdSize = new Size(Math.Min(1920, image.Width), Math.Min(1080, image.Height));
        var hdSize = new Size(1280, 720);

        string fullHdFile = Path.Combine(_options.ImagePath, $"{Path.GetFileNameWithoutExtension(fileName)}_fullhd.webp");
        string hdFile = Path.Combine(_options.ImagePath, $"{Path.GetFileNameWithoutExtension(fileName)}_hd.webp");

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = fullHdSize,
            Mode = ResizeMode.Max
        }));
        await image.SaveAsync(fullHdFile, new WebpEncoder());

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = hdSize,
            Mode = ResizeMode.Max
        }));
        await image.SaveAsync(hdFile, new WebpEncoder());

        return (fullHdFile, hdFile);
    }

    public async Task<string> SaveVideoAsync(Stream videoStream, string fileName)
    {
        var inputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(fileName));
        var outputPath = Path.Combine(_options.VideoPath, Path.ChangeExtension(fileName, ".webm"));

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
        return outputPath;
    }

    public void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
