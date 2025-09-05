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
    public async Task<ActionResult> Login([FromForm] LoginDto loginDto)
    {
        var deviceInfo = DeviceInfoHelper.GetDeviceInfo(Request);

        var result = await _authService.LoginAsync(loginDto, deviceInfo);
        if (result == null)
            return Unauthorized("Invalid username, email, or password");

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromForm] RegisterDto registerDto)
    {
        var deviceInfo = DeviceInfoHelper.GetDeviceInfo(Request);
        var result = await _authService.RegisterAsync(registerDto, deviceInfo);
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
    public async Task<ActionResult> SendEmailVerificationCode()
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

    [Authorize]
    [HttpGet("check-login")]
    public ActionResult CheckLogin()
    {
        return Ok();
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet("check-admin")]
    public ActionResult CheckAdmin()
    {
        return Ok();
    }

    [HttpPost("send-password-reset-code")]
    public async Task<ActionResult<string>> SendPasswordResetCode(string login)
    {
        var result = await _authService.SendPasswordReset(login);
        if (result == null)
            return BadRequest();
        return Ok(result);
    }

    [HttpGet("verify-password-reset-code")]
    public async Task<ActionResult<string>> VerifyPasswordResetCode(string resetToken, string code)
    {
        var result = await _authService.VerifyPasswordCodeAsync(resetToken, code);
        if (result == null)
            return BadRequest();
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(string password, string accessCode)
    {
        var result = await _authService.ResetPassword(password, accessCode);
        return result ? Ok() : BadRequest();
    }
}