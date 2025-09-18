using App.Core.Enums;

namespace App.Core.DTOs;

public class FileArrayItemDto
{
    public Stream Stream { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
    public MediaType Type { get; set; } = MediaType.Unknown;
    public string? Url { get; set; } = string.Empty;
    public string? SecondUrl { get; set; } = string.Empty;
}