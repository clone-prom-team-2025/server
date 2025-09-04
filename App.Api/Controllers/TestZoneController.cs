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
    
    
}