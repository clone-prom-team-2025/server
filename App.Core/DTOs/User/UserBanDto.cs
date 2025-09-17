using App.Core.Enums;

namespace App.Core.DTOs.User;

public class UserBanDto
{
    public string Id { get; set; }

    public string UserId { get; set; }

    public string AdminId { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime BannedAt { get; set; } = DateTime.UtcNow;

    public DateTime? BannedUntil { get; set; }

    public BanType Types { get; set; } = BanType.Comments;
}