namespace App.Core.Interfaces;

public interface IRedisService
{
    Task SetValueAsync(string key, string value, TimeSpan? expiry = null);
    Task<string?> GetValueAsync(string key);
    Task SetObjectAsync<T>(string key, T obj, TimeSpan? expiry = null);
    Task<T?> GetObjectAsync<T>(string key);
}