namespace App.Core.Models.FileStorage;

public class FileStorageOptions
{
    public string ImagePath { get; set; } = null!;
    public string VideoPath { get; set; } = null!;
    public string? FfmpegPath { get; set; }
}