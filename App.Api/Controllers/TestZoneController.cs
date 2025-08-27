using System.Net;
using App.Core.Constants;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UAParser;

namespace App.Api.Controllers;

[ApiController]
[Route("api/test-zone")]
public class TestZoneController : ControllerBase
{
    private readonly ILogger<TestZoneController> _logger;
    
    public TestZoneController(ILogger<TestZoneController> logger)
    {
        _logger = logger;
    }
    
    [Authorize]
    [HttpGet("check-login")]
    public ActionResult CheckLogin()
    {
        return Ok("Authorized");
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet("check-admin")]
    public ActionResult CheckAdmin()
    {
        return Ok("Authorized");
    }

    [Authorize(Roles = RoleNames.User)]
    [HttpGet("check-user")]
    public ActionResult CheckUser()
    {
        return Ok("Authorized");
    }

    [HttpGet("TestBan")]
    [Authorize]
    public IActionResult TestBan()
    {
        var blockedClaim = User.Claims.FirstOrDefault(c => c.Type == "Blocked")?.Value;
        return Ok(blockedClaim);
    }

    [HttpGet("device-info")]
    public IActionResult GetClientInfo()
    {
        // 1. Отримуємо IP клієнта (X-Forwarded-For або RemoteIpAddress)
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? HttpContext.Connection.RemoteIpAddress?.ToString();

        // 2. Парсимо User-Agent
        var userAgent = Request.Headers["User-Agent"].ToString();
        var uaParser = Parser.GetDefault();
        var clientInfo = uaParser.Parse(userAgent);

        var browser = $"{clientInfo.UA.Family} {clientInfo.UA.Major}";
        var os = clientInfo.OS.Family;
        var device = clientInfo.Device.Family ?? "Other";

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
                _logger.LogInformation($"Ip: {ip}, Country: {country}, City: {city}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("GeoIP error: {Message}", ex.Message);
            //Console.WriteLine($"GeoIP error: {ex.Message}");
        }

        return Ok(new
        {
            Ip = ip,
            Country = country,
            City = city,
            Browser = browser,
            OS = os,
            Device = device
        });
    }
}