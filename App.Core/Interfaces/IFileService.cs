namespace App.Core.Interfaces;

public interface IFileService
{
    Task<(string SourceUrl, string CompressedUrl, string SourceName, string CompressedFileName)> SaveImageAsync(Stream imageStream, string fileName, string key);
    Task<(string Url, string FileName)> SaveVideoAsync(Stream videoStream, string fileName, string key);
    Task DeleteFileAsync(string key, string fileName);
}
