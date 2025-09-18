using System.Security.Claims;
using App.Core.Constants;
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
    private readonly ILogger<NotificationController> _logger;
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetAllNotifications()
    {
        using (_logger.BeginScope("GetAllNotificationsAsync action"))
        {
            _logger.LogInformation("GetAllNotificationsAsync called");
            var result = await _notificationService.GetAllNotificationsAsync();
            _logger.LogInformation("GetAllNotificationsAsync success");
            return Ok(result);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNotificationById([FromQuery] string id)
    {
        using (_logger.BeginScope("GetAllNotificationById action"))
        {
            _logger.LogInformation("GetAllNotificationById called with Id={id}", id);
            var result = await _notificationService.GetNotificationAsync(id);
            _logger.LogInformation("GetAllNotificationById success");
            return Ok(result);
        }
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> GetAllNotificationsByUserId([FromQuery] string userId)
    {
        using (_logger.BeginScope("GetAllNotificationsByUserId action"))
        {
            _logger.LogInformation("GetAllNotificationsByUserId called with UserId={userId}", userId);
            var result = await _notificationService.GetAllNotificationsByUserIdAsync(userId);
            _logger.LogInformation("GetAllNotificationsByUserId success");
            return Ok(result);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNotificationsByMyId()
    {
        using (_logger.BeginScope("GetAllNotificationsByUserId action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("GetAllNotificationsByUserId called with UserId={userId}", userId);
            var result = await _notificationService.GetAllNotificationsByUserIdAsync(userId);
            _logger.LogInformation("GetAllNotificationsByUserId success");
            return Ok(result);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSeenNotifications()
    {
        using (_logger.BeginScope("GetSeenNotifications action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("GetSeenNotifications called with UserId={userId}", userId);
            var result = await _notificationService.GetSeenNotificationsAsync(userId);
            _logger.LogInformation("GetSeenNotifications success");
            return Ok(result);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUnSeenNotificationsAsync()
    {
        using (_logger.BeginScope("GetUnSeenNotificationsAsync action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("GetUnSeenNotificationsAsync called with UserId={userId}", userId);
            var result = await _notificationService.GetUnSeenNotificationsAsync(userId);
            _logger.LogInformation("GetUnSeenNotificationsAsync success");
            return Ok(result);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendNotification([FromForm] NotificationCreateDto notification)
    {
        using (_logger.BeginScope("SendNotification action"))
        {
            _logger.LogInformation("SendNotification called");
            await _notificationService.SendNotificationAsync(notification);
            _logger.LogInformation("SendNotification success");
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<IActionResult> SeeNotification(string notificationId)
    {
        using (_logger.BeginScope("SeeNotification action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("SeeNotification called with Id={notificationId}, UserId={userId}", notificationId,
                userId);
            await _notificationService.SeeNotificationAsync(notificationId, userId);
            _logger.LogInformation("SeeNotification success");
            return NoContent();
        }
    }
}