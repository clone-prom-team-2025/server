namespace App.Core.Models.User;

public class UserAvatar(string url, string fileName)
{
    public string Url { get; set; } = url;
    public string FileName { get; set; } = fileName;
}