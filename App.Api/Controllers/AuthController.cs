using System.Security.Claims;
using App.Core.Constants;
using App.Core.DTOs.Auth;
using App.Core.Interfaces;
using App.Core.Utils;
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
    public async Task<IActionResult> Login([FromForm] LoginDto loginDto)
    {
        var deviceInfo = DeviceInfoHelper.GetDeviceInfo(Request);

        var result = await _authService.LoginAsync(loginDto, deviceInfo);
        if (result == null)
            return Unauthorized("Invalid username, email, or password");

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
    {
        var deviceInfo = DeviceInfoHelper.GetDeviceInfo(Request);
        var result = await _authService.RegisterAsync(registerDto, deviceInfo);
        if (result == null)
            return Unauthorized("Invalid username, email, or password");
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var sessionId = User.FindFirstValue(ClaimTypes.Sid);
        if (string.IsNullOrEmpty(sessionId))
            return Unauthorized("Session ID not found in token");

        await _authService.LogoutAsync(sessionId);

        return NoContent();
    }
    
    [HttpGet("active-sessions")]
    [Authorize]
    public async Task<IActionResult> GetActiveSessions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return BadRequest();
        
        return Ok(await _authService.GetActiveSessions(userId));
    }

    [HttpPost("revoke-session")]
    [Authorize]
    public async Task<IActionResult> RevokeSession(string sessionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return BadRequest();
        
        await _authService.LogoutAsync(sessionId, userId);
        
        return NoContent();
    }

    [Authorize]
    [HttpPost("send-email-verification-code")]
    public async Task<IActionResult> SendEmailVerificationCode()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Session ID not found in token");

        await _authService.SendEmailVerificationCodeAsync(userId);
        return Ok("Email verification code sent");
    }

    [Authorize]
    [HttpGet("verify-email-code")]
    public async Task<IActionResult> VerifyEmailCode(string code)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Session ID not found in token");

        await _authService.VerifyCode(userId, code);
        return NoContent();
    }

    [Authorize]
    [HttpGet("check-login")]
    public IActionResult CheckLogin()
    {
        return Ok();
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet("check-admin")]
    public IActionResult CheckAdmin()
    {
        return Ok();
    }

    [HttpPost("send-password-reset-code")]
    public async Task<IActionResult> SendPasswordResetCode(string login)
    {
        var result = await _authService.SendPasswordReset(login);
        if (result == null)
            return BadRequest();
        return Ok(result);
    }

    [HttpGet("verify-password-reset-code")]
    public async Task<IActionResult> VerifyPasswordResetCode(string resetToken, string code)
    {
        var result = await _authService.VerifyPasswordCodeAsync(resetToken, code);
        if (result == null)
            return BadRequest();
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(string password, string accessCode)
    {
        await _authService.ResetPassword(password, accessCode);
        return NoContent();
    }
}