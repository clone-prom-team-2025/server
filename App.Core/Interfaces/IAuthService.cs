using App.Core.DTOs.Auth;

namespace App.Core.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginDto model);
    Task<string?> RegisterAsync(RegisterDto model);
    Task<bool> LogoutAsync(string sessionId);
    Task<bool> SendEmailVerificationCodeAsync(string userId);
    Task<bool> VerifyCode(string userId, string inputCode);
}