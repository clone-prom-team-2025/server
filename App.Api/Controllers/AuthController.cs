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
    ILogger<AuthController> _logger;

    public AuthController(IAuthService authService,  ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] LoginDto loginDto)
    {
        using (_logger.BeginScope("Login")) {
            _logger.LogInformation("Login action");
            var deviceInfo = DeviceInfoHelper.GetDeviceInfo(Request);

            var result = await _authService.LoginAsync(loginDto, deviceInfo);
            if (result == null)
                return Unauthorized("Invalid username, email, or password");
            _logger.LogInformation("Login success");

            return Ok(result);
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
    {
        using (_logger.BeginScope("Register")) {
            _logger.LogInformation("Register action");
            var deviceInfo = DeviceInfoHelper.GetDeviceInfo(Request);
            var result = await _authService.RegisterAsync(registerDto, deviceInfo);
            if (result == null)
                return Unauthorized("Invalid username, email, or password");
            _logger.LogInformation("Register success");
            return Ok(result);
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        using (_logger.BeginScope("Logout")) {
            _logger.LogInformation("Logout action");
            var sessionId = User.FindFirstValue(ClaimTypes.Sid);
            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized("Session ID not found in token");

            await _authService.LogoutAsync(sessionId);
            _logger.LogInformation("Logout success");

            return NoContent();
        }
    }

    [HttpGet("active-sessions")]
    [Authorize]
    public async Task<IActionResult> GetActiveSessions()
    {
        using (_logger.BeginScope("GetActiveSessions")) {
            _logger.LogInformation("GetActiveSessions action");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            
            _logger.LogInformation("GetActiveSessions success");
            return Ok(await _authService.GetActiveSessions(userId));
        }
    }

    [HttpPost("revoke-session")]
    [Authorize]
    public async Task<IActionResult> RevokeSession(string sessionId)
    {
        using (_logger.BeginScope("RevokeSession")) {
            _logger.LogInformation("RevokeSession action");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();

            await _authService.LogoutAsync(sessionId, userId);
            _logger.LogInformation("RevokeSession success");

            return NoContent();
        }
    }

    [Authorize]
    [HttpPost("send-email-verification-code")]
    public async Task<IActionResult> SendEmailVerificationCode()
    {
        using (_logger.BeginScope("SendEmailVerificationCode")) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Session ID not found in token");
            _logger.LogInformation("SendEmailVerificationCode action");

            await _authService.SendEmailVerificationCodeAsync(userId);
            _logger.LogInformation("SendEmailVerificationCode success");
            return Ok("Email verification code sent");
        }
    }

    [Authorize]
    [HttpGet("verify-email-code")]
    public async Task<IActionResult> VerifyEmailCode(string code)
    {
        using (_logger.BeginScope("VerifyEmailCode")) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Session ID not found in token");
            _logger.LogInformation("VerifyEmailCode action");

            await _authService.VerifyCode(userId, code);
            _logger.LogInformation("VerifyEmailCode success");
            return NoContent();
        }
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
        using  (_logger.BeginScope("SendPasswordResetCode")) {
            _logger.LogInformation("SendPasswordResetCode action");
            var result = await _authService.SendPasswordReset(login);
            if (result == null)
                return BadRequest();
            _logger.LogInformation("SendPasswordResetCode success");
            return Ok(result);
        }
    }

    [HttpGet("verify-password-reset-code")]
    public async Task<IActionResult> VerifyPasswordResetCode(string resetToken, string code)
    {
        using (_logger.BeginScope("VerifyPasswordResetCode")) {
            _logger.LogInformation("VerifyPasswordResetCode action");
            var result = await _authService.VerifyPasswordCodeAsync(resetToken, code);
            if (result == null)
                return BadRequest();
            _logger.LogInformation("VerifyPasswordResetCode success");
            return Ok(result);
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(string password, string accessCode)
    {
        using (_logger.BeginScope("ResetPassword")) {
            _logger.LogInformation("ResetPassword action");
            await _authService.ResetPassword(password, accessCode);
            _logger.LogInformation("ResetPassword success");
            return NoContent();
        }
    }
}