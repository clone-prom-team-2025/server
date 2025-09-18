using App.Core.DTOs.Notification;
using App.Core.Interfaces;
using App.Core.Models.Notification;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services.Services;

/// <summary>
/// Handles CRUD operations for notifications, including sending,
/// marking as seen, and deleting notifications for users.
/// </summary>
public class NotificationService(
    INotificationRepository notificationRepository,
    IMapper mapper,
    ILogger<NotificationService> logger,
    IUserRepository userRepository,
    INotificationHubNotifier notificationHubNotifier) : INotificationService
{
    private readonly ILogger<NotificationService> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly INotificationHubNotifier _notificationHubNotifier = notificationHubNotifier;
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly IUserRepository _userRepository = userRepository;

    /// <summary>
    /// Retrieves all notifications in the system.
    /// Throws KeyNotFoundException if no notifications exist.
    /// </summary>
    public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync()
    {
        using (_logger.BeginScope("GetAllNotificationsAsync"))
        {
            _logger.LogInformation("GetAllNotificationsAsync called");
            var result = await _notificationRepository.GetAllNotificationsAsync();
            if (result == null) throw new KeyNotFoundException("No notifications found");
            _logger.LogInformation("GetAllNotificationsAsync completed");
            return _mapper.Map<IEnumerable<NotificationDto>>(result);
        }
    }

    /// <summary>
    /// Retrieves a single notification by its ID.
    /// Throws KeyNotFoundException if the notification does not exist.
    /// </summary>
    public async Task<NotificationDto> GetNotificationAsync(string id)
    {
        using (_logger.BeginScope("GetNotificationAsync: Id={id}", id))
        {
            _logger.LogInformation("GetNotificationAsync called with Id={id}", id);
            var result = await _notificationRepository.GetNotificationByIdAsync(ObjectId.Parse(id));
            if (result == null) throw new KeyNotFoundException("Notification not found");
            _logger.LogInformation("GetNotificationAsync completed");
            return _mapper.Map<NotificationDto>(result);
        }
    }

    /// <summary>
    /// Retrieves all notifications for a specific user by user ID.
    /// Throws KeyNotFoundException if no notifications exist.
    /// </summary>
    public async Task<IEnumerable<NotificationDto>> GetAllNotificationsByUserIdAsync(string userId)
    {
        using (_logger.BeginScope("GetAllNotificationsByUserIdAsync: UserId={userId}", userId))
        {
            _logger.LogInformation("GetAllNotificationsByUserIdAsync called with UserId={userId}", userId);
            var result = await _notificationRepository.GetAllNotificationsByUserIdAsync(ObjectId.Parse(userId));
            if (result == null) throw new KeyNotFoundException("No notifications found");
            _logger.LogInformation("GetAllNotificationsByUserIdAsync completed");
            return _mapper.Map<IEnumerable<NotificationDto>>(result);
        }
    }

    /// <summary>
    /// Retrieves all notifications that have been seen by a specific user.
    /// Throws KeyNotFoundException if no notifications exist.
    /// </summary>
    public async Task<IEnumerable<NotificationDto>> GetSeenNotificationsAsync(string userId)
    {
        using (_logger.BeginScope("GetSeenNotificationsAsync: UserId={userId}", userId))
        {
            _logger.LogInformation("GetSeenNotificationsAsync called with UserId={userId}", userId);
            var result = await _notificationRepository.GetSeenNotificationsAsync(ObjectId.Parse(userId));
            if (result == null) throw new KeyNotFoundException("No notifications found");
            _logger.LogInformation("GetSeenNotificationsAsync completed");
            return _mapper.Map<IEnumerable<NotificationDto>>(result);
        }
    }

    /// <summary>
    /// Retrieves all notifications that have not been seen by a specific user.
    /// Throws KeyNotFoundException if no notifications exist.
    /// </summary>
    public async Task<IEnumerable<NotificationDto>> GetUnSeenNotificationsAsync(string userId)
    {
        using (_logger.BeginScope("GetSeenNotificationsByUserIdAsync: UserId={userId}", userId))
        {
            _logger.LogInformation("GetSeenNotificationsByUserIdAsync called with UserId={userId}", userId);
            var result = await _notificationRepository.GetUnseenNotificationsAsync(ObjectId.Parse(userId));
            if (result == null) throw new KeyNotFoundException("No notifications found");
            _logger.LogInformation("GetSeenNotificationsByUserIdAsync completed");
            return _mapper.Map<IEnumerable<NotificationDto>>(result);
        }
    }

    /// <summary>
    /// Sends a notification by creating it in the repository and notifying via hub.
    /// </summary>
    public async Task SendNotificationAsync(NotificationCreateDto notificationCreateDto)
    {
        using (_logger.BeginScope("CreateNotificationAsync: notificationCreateDto"))
        {
            _logger.LogInformation("CreateNotificationAsync called");
            var notification = _mapper.Map<Notification>(notificationCreateDto);
            notification.Id = ObjectId.GenerateNewId();
            notification.CreatedAt = DateTime.UtcNow;
            await _notificationRepository.CreateNotificationAsync(notification);
            _logger.LogInformation("CreateNotificationAsync completed");
            var notificationDto = _mapper.Map<NotificationDto>(notification);
            await _notificationHubNotifier.SendNotificationAsync(notificationDto);
            _logger.LogInformation("CreateNotificationAsync notification sent");
        }
    }

    /// <summary>
    /// Deletes a notification by its ID.
    /// Throws KeyNotFoundException if the notification does not exist.
    /// </summary>
    public async Task DeleteNotificationAsync(string id)
    {
        using (_logger.BeginScope("DeleteNotificationAsync: id={id}", id))
        {
            _logger.LogInformation("DeleteNotificationAsync called with Id={id}", id);
            var result = await _notificationRepository.DeleteNotificationAsync(ObjectId.Parse(id));
            if (!result) throw new KeyNotFoundException("Notification not found");
            _logger.LogInformation("DeleteNotificationAsync completed");
        }
    }

    /// <summary>
    /// Deletes all notifications in the system.
    /// Throws KeyNotFoundException if no notifications exist.
    /// </summary>
    public async Task DeleteAllNotificationsAsync(string userId)
    {
        using (_logger.BeginScope("DeleteAllNotificationsAsync: userId={userId}", userId))
        {
            _logger.LogInformation("DeleteAllNotificationsAsync called");
            var result = await _notificationRepository.DeleteAllNotificationsAsync();
            if (!result) throw new KeyNotFoundException("No notifications found");
            _logger.LogInformation("DeleteAllNotificationsAsync completed");
        }
    }

    /// <summary>
    /// Deletes all notifications for a specific user.
    /// Throws KeyNotFoundException if no notifications exist for the user.
    /// </summary>
    public async Task DeleteAllNotificationsByUserIdAsync(string userId)
    {
        using (_logger.BeginScope("DeleteAllNotificationsByUserIdAsync: userId={userId}", userId))
        {
            _logger.LogInformation("DeleteAllNotificationsByUserIdAsync called with UserId={userId}", userId);
            var result = await _notificationRepository.DeleteAllNotificationsByUserIdAsync(ObjectId.Parse(userId));
            if (!result) throw new KeyNotFoundException("No notifications found");
            _logger.LogInformation("DeleteAllNotificationsByUserIdAsync completed");
        }
    }

    // public async Task CreateSeenNotificationAsync(NotificationCreateDto notificationSeen)
    // {
    //     using (_logger.BeginScope("CreateSeenNotificationAsync: notificationSeen"))
    //     {
    //         _logger.LogInformation("CreateSeenNotificationAsync called");
    //         var notification = _mapper.Map<NotificationSeen>(notificationSeen);
    //         notification.Id = ObjectId.GenerateNewId();
    //         notification.SeenAt = DateTime.UtcNow;
    //         await _notificationRepository.CreateSeenNotificationAsync(notification);
    //         _logger.LogInformation("CreateSeenNotificationAsync completed");
    //     }
    // }

    /// <summary>
    /// Deletes a seen notification by ID.
    /// Throws KeyNotFoundException if the notification does not exist.
    /// </summary>
    public async Task DeleteSeenNotificationAsync(string id)
    {
        using (_logger.BeginScope("DeleteSeenNotificationAsync: id={id}", id))
        {
            _logger.LogInformation("DeleteSeenNotificationAsync called");
            var result = await _notificationRepository.DeleteSeenNotificationAsync(ObjectId.Parse(id));
            if (!result) throw new KeyNotFoundException("Notification not found");
            _logger.LogInformation("DeleteSeenNotificationAsync completed");
        }
    }

    /// <summary>
    /// Deletes all seen notifications in the system.
    /// Throws KeyNotFoundException if no notifications exist.
    /// </summary>
    public async Task DeleteAllSeenNotificationsAsync()
    {
        using (_logger.BeginScope("DeleteAllSeenNotificationsAsync: "))
        {
            _logger.LogInformation("DeleteAllSeenNotificationsAsync called");
            var result = await _notificationRepository.DeleteAllSeenNotificationsAsync();
            if (!result) throw new KeyNotFoundException("No notifications found");
            _logger.LogInformation("DeleteAllSeenNotificationsAsync completed");
        }
    }

    /// <summary>
    /// Deletes all seen notifications for a specific user.
    /// Throws KeyNotFoundException if no notifications exist for the user.
    /// </summary>
    public async Task DeleteAllSeenNotificationsByUserIdAsync(string userId)
    {
        using (_logger.BeginScope("DeleteAllSeenNotificationsByUserIdAsync: userId={userId}", userId))
        {
            _logger.LogInformation("DeleteAllSeenNotificationsByUserIdAsync called");
            var result = await _notificationRepository.DeleteAllSeenNotificationsByUserIdAsync(ObjectId.Parse(userId));
            if (!result) throw new KeyNotFoundException("No notifications found");
            _logger.LogInformation("DeleteAllSeenNotificationsByUserIdAsync completed");
        }
    }

    /// <summary>
    /// Deletes all seen notifications associated with a specific notification ID.
    /// Throws KeyNotFoundException if no notifications exist.
    /// </summary>
    public async Task DeleteAllSeenNotificationsByNotificationIdAsync(string notificationId)
    {
        using (_logger.BeginScope(
                   "DeleteAllSeenNotificationsByNotificationIdAsync called with NotificationId={notificationId}",
                   notificationId))
        {
            _logger.LogInformation("DeleteAllSeenNotificationsByNotificationIdAsync called");
            var result =
                await _notificationRepository.DeleteAllSeenNotificationsByNotificationIdAsync(
                    ObjectId.Parse(notificationId));
            if (!result) throw new KeyNotFoundException("No notifications found");
            _logger.LogInformation("DeleteAllSeenNotificationsByNotificationIdAsync completed");
        }
    }

    /// <summary>
    /// Marks a notification as seen by a user.
    /// Throws KeyNotFoundException if the notification or user does not exist.
    /// Throws InvalidOperationException if the notification was already marked as seen.
    /// </summary>
    public async Task SeeNotificationAsync(string notificationId, string userId)
    {
        using (_logger.BeginScope("SeeNotificationAsync: notificationId={notificationId}", notificationId))
        {
            _logger.LogInformation("SeeNotificationAsync called with NotificationId={notificationId}", notificationId);
            var notification = await _notificationRepository.GetNotificationByIdAsync(ObjectId.Parse(notificationId));
            if (notification == null) throw new KeyNotFoundException("Notification not found");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null) throw new KeyNotFoundException("User not found");
            if (!await _notificationRepository.HasSeenNotificationAsync(user.Id, notification.Id))
                throw new InvalidOperationException("Notification already seen");
            var seen = new NotificationSeen
            {
                Id = ObjectId.GenerateNewId(),
                NotificationId = notification.Id,
                SeenAt = DateTime.UtcNow,
                UserId = user.Id
            };
            await _notificationRepository.CreateSeenNotificationAsync(seen);
            _logger.LogInformation("SeeNotificationAsync completed");
        }
    }
}