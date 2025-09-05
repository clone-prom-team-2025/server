using Microsoft.AspNetCore.Mvc;

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