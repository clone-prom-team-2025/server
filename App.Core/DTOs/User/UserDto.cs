using App.Core.Models.FileStorage;

namespace App.Core.DTOs.User;

public class UserDto
{
    public string Id { get; set; }

    public string Username { get; set; }

    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string? MiddleName { get; set; }

    public string? PhoneNumber { get; set; }

    public bool? PhoneNumberConfirmed { get; set; } = false;

    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public BaseFile Avatar { get; set; }

    public List<string> Roles { get; set; }

    public DateTime CreatedAt { get; set; }
}