using Microsoft.AspNetCore.Http;

namespace App.Core.DTOs.User;

public class UserCreateAdminDto
{
    public string FullNmae { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Username { get; set; }
    public string Password { get; set; } = null!;
    public IFormFile? Avatar { get; set; } = null!;
}