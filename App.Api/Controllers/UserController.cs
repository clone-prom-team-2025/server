using System.Security.Claims;
using App.Core.Constants;
using App.Core.DTOs.User;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    
    [HttpPost("ban")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> BanUser([FromForm] UserBanCreateDto userBlockInfo)
    {
        using (_logger.BeginScope("BanUser action"))
        {
            _logger.LogInformation("BanUser called with UserId={UserId}", userBlockInfo.UserId);

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null)
            {
                _logger.LogError("AdminId claim is missing");
                return BadRequest("AdminId claim is missing");
            }

            _logger.LogInformation("AdminId: {AdminId}", adminIdClaim.Value);

            var result = await _userService.BanUser(userBlockInfo, adminIdClaim.Value);
            if (!result)
            {
                _logger.LogWarning("BanUser failed for UserId={UserId}", userBlockInfo.UserId);
                return BadRequest("Failed to ban user");
            }

            _logger.LogInformation("UserId={UserId} was successfully banned by AdminId={AdminId}", 
                userBlockInfo.UserId, adminIdClaim.Value);

            return Ok();
        }
    }
    
    [HttpPost("unban/{banId}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> UnbanUser(string banId)
    {
        using (_logger.BeginScope("UnbanUser action"))
        {
            _logger.LogInformation("UnbanUser called with BanId={BanId}", banId);

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null)
            {
                _logger.LogError("AdminId claim is missing");
                return BadRequest("AdminId claim is missing");
            }

            _logger.LogInformation("AdminId: {AdminId}", adminIdClaim.Value);

            var result = await _userService.UnbanUserByBanId(banId, adminIdClaim.Value);
            if (!result)
            {
                _logger.LogWarning("UnbanUser failed for BanId={BanId}", banId);
                return BadRequest("Failed to unban user");
            }

            _logger.LogInformation("BanId={BanId} successfully unbanned by AdminId={AdminId}", banId, adminIdClaim.Value);
            return Ok();
        }
    }

    [HttpGet("ban/{userId}")]
    public async Task<IEnumerable<UserBanDto>> GetAllByUserId(string userId)
    {
        using (_logger.BeginScope("GetAllByUserId action"))
        {
            _logger.LogInformation("GetAllByUserId called with UserId={UserId}", userId);
            var bans = await _userService.GetUserBansByUserId(userId);
            var allByUserId = bans as UserBanDto[] ?? bans.ToArray();
            _logger.LogInformation("Found {Count} bans for UserId={UserId}", allByUserId.Count(), userId);
            return allByUserId;
        }
    }

    [HttpGet("ban/my")]
    public async Task<IEnumerable<UserBanDto>> GetAllByMyUserId()
    {
        using (_logger.BeginScope("GetAllByMyUserId action"))
        {
            _logger.LogInformation("GetAllByMyUserId called");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return new List<UserBanDto>();
            }

            _logger.LogInformation("Fetching bans for current UserId={UserId}", userIdClaim.Value);
            var bans = await _userService.GetUserBansByUserId(userIdClaim.Value);
            var allByMyUserId = bans as UserBanDto[] ?? bans.ToArray();
            _logger.LogInformation("Found {Count} bans for current UserId={UserId}", allByMyUserId.Count(), userIdClaim.Value);
            return allByMyUserId;
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<UserDto?> GetUserById(string userId)
    {
        using (_logger.BeginScope("GetUserById action"))
        {
            _logger.LogInformation("GetUserById called with UserId={UserId}", userId);
            _logger.LogInformation("Fetching user for current UserId={UserId}", userId);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for UserId={UserId}", userId);
                return null;
            }
            _logger.LogInformation("Found user for current UserId={UserId}", userId);
            return user;
        }
    }

    [HttpGet("users")]
    public async Task<IEnumerable<UserDto>> GetAllUsers()
    {
        using (_logger.BeginScope("GetAllUsers action"))
        {
            _logger.LogInformation("GetAllUsers called");
            var users = await _userService.GetAllUsersAsync();
            var  allUsers = users as UserDto[] ?? users.ToArray();
            _logger.LogInformation("Found {Count} users", allUsers.Length);
            return allUsers;
        }
    }

    [HttpGet("user/my")]
    public async Task<UserDto?> GetUserById()
    {
        using (_logger.BeginScope("GetUserById action"))
        {
            _logger.LogInformation("GetUserById called");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("UserId claim is missing");
                return null;
            }
            _logger.LogInformation("Fetching user for current UserId={UserId}", userId);
            var user = await _userService.GetUserByIdAsync(userId.Value);
            _logger.LogInformation("Found user for current UserId={UserId}", userId);
            return user;
        }
    }
}