using System.Text.Json;

namespace App.Core.Utils;

public static class RedisExtensions
{
    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    public static T? Deserialize<T>(string value)
    {
        return JsonSerializer.Deserialize<T>(value);
    }
}