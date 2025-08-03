using App.Core.DTOs.Auth;
using App.Core.Models.Auth;

namespace App.Core.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginDto model);
    Task<string?> RegisterAsync(RegisterDto model);
}