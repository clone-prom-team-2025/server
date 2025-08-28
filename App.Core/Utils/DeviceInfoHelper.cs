using System.Net;
using App.Core.Models.User;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Http;
using UAParser;

namespace App.Core.Utils;

public static class DeviceInfoHelper
{
    public static DeviceInfo GetDeviceInfo(HttpRequest request)
    {
        var ip = request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? request.HttpContext.Connection.RemoteIpAddress?.ToString();

        var userAgent = request.Headers["User-Agent"].ToString();
        var uaParser = Parser.GetDefault();
        var clientInfo = uaParser.Parse(userAgent);

        var browser = $"{clientInfo.UA.Family} {clientInfo.UA.Major}";
        var os = clientInfo.OS.Family;
        var device = clientInfo.Device.Family;

        if (string.IsNullOrEmpty(device) || device == "Other")
        {
            device = clientInfo.OS.Family switch
            {
                "Windows" => "Desktop",
                "Linux"   => "Desktop",
                "macOS"   => "Desktop",
                "Android" => "Mobile",
                "iOS"     => "Mobile",
                _         => "Other"
            };
        }

        // 3. Геолокація через GeoIP2
        var country = "Unknown";
        var city = "Unknown";

        try
        {
            if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out var ipAddr))
            {
                using var reader = new DatabaseReader(Path.Combine(AppContext.BaseDirectory, "database", "GeoLite2-City.mmdb"));
                var response = reader.City(ipAddr);

                country = response?.Country?.Name ?? "Unknown";
                city = response?.City?.Name ?? "Unknown";
            }
        }
        catch
        {
            Console.Error.WriteLine("Unable to parse ip address");
        }

        return new DeviceInfo
        {
            Ip = ip ?? "Unknown",
            Browser = browser,
            Os = os,
            Device = device,
            Country = country,
            City = city
        };
    }
}