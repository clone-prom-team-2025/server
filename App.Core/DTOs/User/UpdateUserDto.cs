using Microsoft.AspNetCore.Http;

namespace App.Core.DTOs.User;

public class UpdateUserDto
{
    public string? Username { get; set; } = null;
    public string? FullName { get; set; } = null;
    public string? Gender { get; set; } = null;
    public DateTime? DateOfBirth { get; set; } = null;
    public IFormFile? Avatar { get; set; } = null;
}