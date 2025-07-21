namespace App.Core.Interfaces;

public interface IFileService
{
    Task<(string FullHdPath, string HdPath)> SaveImageAsync(Stream imageStream, string fileName);
    Task<string> SaveVideoAsync(Stream videoStream, string fileName);
    void DeleteFile(string path);
}
