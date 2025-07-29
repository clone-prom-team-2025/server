using System;
using System.Collections.Generic;
using App.Core.Enums;

namespace App.Core.DTOs;

public class UserDto
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string AvatarUrl { get; set; }
    public List<UserRole> Roles { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserAdditionalInfo? AdditionalInfo { get; set; }
    public UserBlockInfo? BlockInfo { get; set; }
} 