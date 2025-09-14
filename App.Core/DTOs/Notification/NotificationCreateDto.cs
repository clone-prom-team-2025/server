using App.Core.Enums;

namespace App.Core.DTOs.Notification;

public class NotificationCreateDto
{
    public NotificationType Type { get; set; } = NotificationType.Info;

    public string Message { get; set; } = null!;

    public string? From { get; set; }
    public string? To { get; set; }
    
    public string? MetadataUrl { get; set; }

    public bool IsHighPriority { get; set; } = false;
}