using App.Core.Enums;

namespace App.Core.DTOs.Notification;

public class NotificationDto
{
    public string Id { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.Info;

    public string Message { get; set; } = null!;

    public string? From { get; set; }
    public string? To { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? MetadataUrl { get; set; }

    public bool IsHighPriority { get; set; } = false;
}