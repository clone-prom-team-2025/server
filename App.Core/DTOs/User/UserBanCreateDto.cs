using App.Core.Enums;

namespace App.Core.DTOs.User;

public class UserBanCreateDto
{
    public string UserId { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime? BannedUntil { get; set; }

    public BanType Types { get; set; } = BanType.None;
}