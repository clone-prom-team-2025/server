namespace App.Core.Models.FileStorage;

public class BaseFile
{
    public string SourceFileName { get; set; } = string.Empty;
    public string? CompressedFileName { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string? CompressedUrl { get; set; }
}