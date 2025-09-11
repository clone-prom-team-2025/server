using App.Core.DTOs.Notification;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;
    
    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> SendNotification([FromForm] NotificationCreateDto notification)
    {
        using (_logger.BeginScope("SendNotification"))
        {
            _logger.LogInformation("SendNotification called");
            await _notificationService.SendNotificationAsync(notification);
            _logger.LogInformation("SendNotification completed");
            return NoContent();
        }
    }
}