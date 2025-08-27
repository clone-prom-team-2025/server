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
        // IP клієнта
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? HttpContext.Connection.RemoteIpAddress?.ToString();

        // User-Agent
        var userAgent = Request.Headers["User-Agent"].ToString();
        var uaParser = Parser.GetDefault();
        var clientInfo = uaParser.Parse(userAgent);

        var browser = $"{clientInfo.UA.Family} {clientInfo.UA.Major}";
        var os = clientInfo.OS.Family;
        var device = clientInfo.Device.Family; // iPhone, Samsung, Other

        // Геолокація через GeoIP2
        var country = "Unknown";
        var city = "Unknown";

        try
        {
            using var reader = new DatabaseReader("GeoLite2-City.mmdb");
            if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out var ipAddr))
            {
                var cityResponse = reader.City(ipAddr);
                country = cityResponse.Country.Name;
                city = cityResponse.City.Name;
            }
        }
        catch
        {
            // Якщо база не доступна або IP локальний (::1 / 127.0.0.1)
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