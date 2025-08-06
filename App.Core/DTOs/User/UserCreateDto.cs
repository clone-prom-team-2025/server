using App.Core.Enums;
using App.Core.Models.User;

namespace App.Core.DTOs.User;

public class UserCreateDto
{
    public string Username { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public UserAvatar AvatarUrl { get; set; }
    public List<string> Roles { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserAdditionalInfo? AdditionalInfo { get; set; }
    public UserBlockInfo? BlockInfo { get; set; }
} 