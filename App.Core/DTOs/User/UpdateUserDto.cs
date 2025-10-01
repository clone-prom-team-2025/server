using Microsoft.AspNetCore.Http;

namespace App.Core.DTOs.User;

public class UpdateUserDto
{
    public string? Username { get; set; } = null;
    public string? FirstName { get; set; } = null;
    public string? LastName { get; set; } = null;
    public string? MiddleName { get; set; } = null;
    public string? PhoneNumber { get; set; } = null;
    public string? Gender { get; set; } = null;
    public DateTime? DateOfBirth { get; set; } = null;
    public IFormFile? Avatar { get; set; } = null;
}