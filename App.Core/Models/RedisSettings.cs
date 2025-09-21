namespace App.Core.Models;

public class RedisSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }   
}