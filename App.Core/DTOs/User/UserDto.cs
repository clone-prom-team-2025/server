using App.Core.Enums;
using App.Core.Models.FileStorage;
using App.Core.Models.User;

namespace App.Core.DTOs.User;

public class UserDto
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public BaseFile Avatar { get; set; }
    public List<string> Roles { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserAdditionalInfo? AdditionalInfo { get; set; }
} 