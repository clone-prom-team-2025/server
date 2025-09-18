using App.Core.DTOs.Auth;
using App.Core.Models.User;

namespace App.Core.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginDto model, DeviceInfo deviceInfo);
    Task<string?> RegisterAsync(RegisterDto model, DeviceInfo deviceInfo);
    Task SendEmailVerificationCodeAsync(string userId);
    Task LogoutAsync(string sessionId);
    Task LogoutAsync(string sessionId, string userId);
    Task VerifyCode(string userId, string inputCode);
    Task<string?> SendPasswordReset(string login);
    Task<string?> VerifyPasswordCodeAsync(string resetToken, string inputCode);
    Task ResetPassword(string password, string accessCode);
    Task<IEnumerable<UserSessionDto>?> GetActiveSessions(string userId);
    Task RevokeAllSessions(string userId);
}