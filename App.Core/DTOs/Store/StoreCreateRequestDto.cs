using App.Core.Enums;

namespace App.Core.DTOs.Store;

public class StoreCreateRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ApprovedByAdminId { get; set; } = null;
    public string? RejectedByAdminId { get; set; } = null;
    public List<StoreRequestCommentDto> Comments { get; set; } = [];
    public StorePlans Plan { get; set; } = StorePlans.None;
}