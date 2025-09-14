using System.Security.Claims;
using App.Core.Constants;
using App.Core.DTOs.User;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly ISessionHubNotifier _sessionHubNotifier;
    private readonly IUserService _userService;
    private readonly IUserSessionRepository _userSessionRepository;

    public UserController(IUserService userService, ILogger<UserController> logger,
        IUserSessionRepository userSessionRepository, ISessionHubNotifier sessionHubNotifier)
    {
        _userService = userService;
        _logger = logger;
        _userSessionRepository = userSessionRepository;
        _sessionHubNotifier = sessionHubNotifier;
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> BanUser([FromForm] UserBanCreateDto userBlockInfo)
    {
        using (_logger.BeginScope("BanUser action"))
        {
            _logger.LogInformation("BanUser called with UserId={UserId}", userBlockInfo.UserId);

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null)
            {
                _logger.LogError("UserId claim is missing");
                return BadRequest();
            }

            _logger.LogInformation("AdminId: {AdminId}", adminIdClaim.Value);

            await _userService.BanUser(userBlockInfo, adminIdClaim.Value);

            _logger.LogInformation("UserId={UserId} was successfully banned by AdminId={AdminId}",
                userBlockInfo.UserId, adminIdClaim.Value);

            return Ok();
        }
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> UnbanUser(string banId)
    {
        using (_logger.BeginScope("UnbanUser action"))
        {
            _logger.LogInformation("UnbanUser called with BanId={BanId}", banId);

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null)
            {
                _logger.LogError("UserId claim is missing");
                return BadRequest();
            }

            _logger.LogInformation("AdminId: {AdminId}", adminIdClaim.Value);

            await _userService.UnbanUserByBanId(banId, adminIdClaim.Value);

            _logger.LogInformation("BanId={BanId} successfully unbanned by AdminId={AdminId}", banId,
                adminIdClaim.Value);
            return Ok();
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllBansByUserId(string userId)
    {
        using (_logger.BeginScope("GetAllByUserId action"))
        {
            _logger.LogInformation("GetAllByUserId called with UserId={UserId}", userId);
            var bans = await _userService.GetUserBansByUserId(userId);
            var allByUserId = bans as UserBanDto[] ?? bans.ToArray();
            _logger.LogInformation("Found {Count} bans for UserId={UserId}", allByUserId.Count(), userId);
            return Ok(allByUserId);
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllBansByMyUserId()
    {
        using (_logger.BeginScope("GetAllByMyUserId action"))
        {
            _logger.LogInformation("GetAllByMyUserId called");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            _logger.LogInformation("Fetching bans for current UserId={UserId}", userIdClaim.Value);
            var bans = await _userService.GetUserBansByUserId(userIdClaim.Value);
            var allByMyUserId = bans as UserBanDto[] ?? bans.ToArray();
            _logger.LogInformation("Found {Count} bans for current UserId={UserId}", allByMyUserId.Count(),
                userIdClaim.Value);
            if (allByMyUserId.Length == 0)
                _logger.LogInformation("No bans found for current UserId={UserId}", userIdClaim.Value);

            return Ok(allByMyUserId);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserById(string userId)
    {
        using (_logger.BeginScope("GetUserById action"))
        {
            _logger.LogInformation("GetUserById called with UserId={UserId}", userId);
            _logger.LogInformation("Fetching user for current UserId={UserId}", userId);
            var user = await _userService.GetUserByIdAsync(userId);
            _logger.LogInformation("Found user for current UserId={UserId}", userId);
            return Ok(user);
        }
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> GetAllUsers()
    {
        using (_logger.BeginScope("GetAllUsers action"))
        {
            _logger.LogInformation("GetAllUsers called");
            var users = await _userService.GetAllUsersAsync();
            var allUsers = users as UserDto[] ?? users.ToArray();
            _logger.LogInformation("Found {Count} users", allUsers.Length);
            return Ok(allUsers);
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUserByMyId()
    {
        using (_logger.BeginScope("GetUserById action"))
        {
            _logger.LogInformation("GetUserById called");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("UserId claim is missing");
                return BadRequest();
            }

            _logger.LogInformation("Fetching user for current UserId={UserId}", userId);
            var user = await _userService.GetUserByIdAsync(userId.Value);
            _logger.LogInformation("Found user for current UserId={UserId}", userId);
            return Ok(user);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsersByPages(int pageNumber, int pageSize)
    {
        using (_logger.BeginScope("GetAllUsersByPages action"))
        {
            _logger.LogInformation("GetAllUsersByPages called with PageNumber={pageNumber}, PageSize={pageSize}",
                pageNumber, pageSize);
            var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
            var any = users.ToArray();
            _logger.LogInformation("Found {Count} users", any.Length);
            return Ok(any);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        using (_logger.BeginScope("GetUserByUsername action"))
        {
            _logger.LogInformation("GetUserByUsername called with Username={username}", username);
            var users = await _userService.GetUserByUsernameAsync(username);
            _logger.LogInformation("Found user for current Username={username}", username);
            return Ok(users);
        }
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetUsersByRole(string role)
    {
        using (_logger.BeginScope("GetUsersByRole action"))
        {
            _logger.LogInformation("GetUsersByRole called with Role={role}", role);
            var users = await _userService.GetUsersByRoleAsync(role);
            var any = users.ToArray();
            _logger.LogInformation("Found {Count} users", any.Length);
            return Ok(any);
        }
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetUsersByRoleByPages(string role, int pageNumber,
        int pageSize)
    {
        using (_logger.BeginScope("GetUsersByRoleByPages action"))
        {
            _logger.LogInformation("GetUsersByRoleByPages called with Role={role}", role);
            var users = await _userService.GetUsersByRoleAsync(role, pageNumber, pageSize);
            var any = users.ToArray();
            _logger.LogInformation("Found {Count} users", any.Length);
            return Ok(any);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SendPasswordResetCode()
    {
        using (_logger.BeginScope("SendPasswordResetCode action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("UserId claim is missing");
                return BadRequest();
            }

            _logger.LogInformation("SendPasswordResetCode called with UserId={userId}", userId);

            await _userService.SendDeleteAccountCodeAsync(userId);

            _logger.LogInformation("SendPasswordResetCode succeeded");
            return NoContent();
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteUser(string code)
    {
        using (_logger.BeginScope("DeleteUser action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("UserId claim is missing");
                return BadRequest();
            }

            _logger.LogInformation("DeleteUser called with UserId={userId}", userId);

            var sessions = await _userSessionRepository.GetSessionsAsync(ObjectId.Parse(userId));
            if (sessions != null)
                foreach (var session in sessions)
                    await _sessionHubNotifier.ForceLogoutAsync(session.Id.ToString());
            await _userSessionRepository.DeleteSessionsAsync(ObjectId.Parse(userId));

            await _userService.DeleteUserAsync(userId, code);

            _logger.LogInformation("DeleteUser succeeded");
            return NoContent();
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateUser([FromForm] UpdateUserDto dto)
    {
        using (_logger.BeginScope("UpdateUser action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("UserId claim is missing");
                return BadRequest();
            }

            _logger.LogInformation("UpdateUser called with UserId={userId}", userId);

            await _userService.UpdateUser(userId, dto);

            _logger.LogInformation("UpdateUser succeeded");
            return NoContent();
        }
    }
}