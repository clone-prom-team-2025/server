using App.Core.Enums;
using App.Core.Models.User;
using MongoDB.Bson;

namespace App.Core.DTOs.Auth;

public class UserSessionDto
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DeviceInfo DeviceInfo { get; set; }
    public List<string> Roles { get; set; }
    public BanType Banned { get; set; } = BanType.None;
    public DateTime? BannedUntil { get; set; } = null;
    public bool IsRevoked { get; set; }
}