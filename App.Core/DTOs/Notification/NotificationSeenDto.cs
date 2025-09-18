namespace App.Core.DTOs.Notification;

public class NotificationSeenDto
{
    public string Id { get; set; }

    public string NotificationId { get; set; }

    public string UserId { get; set; }

    public DateTime SeenAt { get; set; } = DateTime.UtcNow;
}