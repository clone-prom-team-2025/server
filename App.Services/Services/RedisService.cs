using App.Core.Interfaces;
using App.Core.Models;
using App.Core.Utils;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace App.Services.Services;

public class RedisService : IRedisService, IDisposable
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService(IOptions<RedisSettings> options)
    {
        var configOptions = new ConfigurationOptions
        {
            EndPoints = { $"{options.Value.Host}:{options.Value.Port}" },
            Password = options.Value.Password,
            AbortOnConnectFail = false
        };

        _redis = ConnectionMultiplexer.Connect(configOptions);
        _db = _redis.GetDatabase();
    }

    public async Task SetValueAsync(string key, string value, TimeSpan? expiry = null)
    {
        await _db.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetValueAsync(string key)
    {
        return await _db.StringGetAsync(key);
    }
    
    public async Task SetObjectAsync<T>(string key, T obj, TimeSpan? expiry = null)
    {
        var json = RedisExtensions.Serialize(obj);
        await _db.StringSetAsync(key, json, expiry);
    }

    public async Task<T?> GetObjectAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? RedisExtensions.Deserialize<T>(value!) : default;
    }

    public void Dispose()
    {
        _redis.Dispose();
    }
}