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
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var sessionId = User.FindFirstValue(ClaimTypes.Sid);
        if (string.IsNullOrEmpty(sessionId))
            return Unauthorized("Session ID not found in token");
        
        var result = await _authService.LogoutAsync(sessionId);
        
        return result ? Ok() : Unauthorized("Invalid username, email, or password");
    }

    [Authorize]
    [HttpPost("send-email-verification-code")]
    public async Task<ActionResult> SendEmailVerificationCode(string? language = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Session ID not found in token");
        
        await _authService.SendEmailVerificationCodeAsync(userId);
        return Ok("Email verification code sent");
    }

    [Authorize]
    [HttpGet("verify-email-code")]
    public async Task<ActionResult> VerifyEmailCode(string code)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Session ID not found in token");
        
        var result = await _authService.VerifyCode(userId, code);
        return result ? Ok() : Unauthorized("Invalid code");
    }
}
