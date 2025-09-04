using App.Core.Enums;
using App.Core.Models.FileStorage;

namespace App.Core.DTOs.Store;

public class StoreCreateRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; }
    public BaseFile Avatar { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ApprovedByAdminId { get; set; } = null;
    public string? RejectedByAdminId { get; set; } = null;
    public List<StoreRequestCommentDto> Comments { get; set; } = [];
    public StorePlans Plan { get; set; } = StorePlans.None;
}