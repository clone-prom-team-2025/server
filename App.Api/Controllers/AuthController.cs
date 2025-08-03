using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using App.Core.DTOs.Auth;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromForm] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
            return Unauthorized("Invalid username, email, or password");
    
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromForm] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        if (result == null)
            return Unauthorized("Invalid username, email, or password");
        return Ok(result);
    }

    [Authorize]
    [HttpGet("check-login")]
    public ActionResult CheckLogin()
    {
        return Ok("Authorized");
    }
}
