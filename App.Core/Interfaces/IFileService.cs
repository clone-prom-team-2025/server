namespace App.Core.Interfaces;

public interface IFileService
{
    Task<(string FullHdUrl, string HdUrl, string UrlFileName, string SecondUrlFileName)> SaveImageFullHdAndHdAsync(Stream imageStream, string fileName, string key);
    Task<(string FullHdUrl, string UrlFileName)> SaveImageFullHdAsync(Stream imageStream, string fileName, string key);
    Task<(string Url, string FileName)> SaveVideoAsync(Stream videoStream, string fileName, string key);
    Task DeleteFileAsync(string key, string fileName);
}
