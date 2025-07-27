namespace App.Core.Interfaces;

public interface IFileService
{
    Task<(string FullHdPath, string HdPath, string fileName)> SaveImageAsync(Stream imageStream, string fileName);
    Task<(string Url, string FileName)> SaveVideoAsync(Stream videoStream, string fileName);
    void DeleteFile(string path);
}
