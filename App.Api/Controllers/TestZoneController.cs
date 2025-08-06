using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Core.Constants;

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
}