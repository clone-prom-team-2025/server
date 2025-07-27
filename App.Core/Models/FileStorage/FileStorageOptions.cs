namespace App.Core.Models.FileStorage;

public class FileStorageOptions
{
    public string FullHdImagePath { get; set; } = default!;
    public string HdImagePath { get; set; } = default!;
    public string VideoPath { get; set; } = default!;
    public string? FfmpegPath { get; set; }
}
