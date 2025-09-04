using System.Security.Claims;
using App.Core.Constants;
using App.Core.DTOs.User;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> BanUser([FromForm] UserBanCreateDto userBlockInfo)
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

            var result = await _userService.BanUser(userBlockInfo, adminIdClaim.Value);
            if (!result)
            {
                _logger.LogWarning("BanUser failed for UserId={UserId}", userBlockInfo.UserId);
                return BadRequest();
            }

            _logger.LogInformation("UserId={UserId} was successfully banned by AdminId={AdminId}",
                userBlockInfo.UserId, adminIdClaim.Value);

            return Ok();
        }
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> UnbanUser(string banId)
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

            var result = await _userService.UnbanUserByBanId(banId, adminIdClaim.Value);
            if (!result)
            {
                _logger.LogWarning("UnbanUser failed for BanId={BanId}", banId);
                return BadRequest();
            }

            _logger.LogInformation("BanId={BanId} successfully unbanned by AdminId={AdminId}", banId,
                adminIdClaim.Value);
            return Ok();
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserBanDto>>> GetAllBansByUserId(string userId)
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

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserBanDto>>> GetAllBansByMyUserId()
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
            {
                _logger.LogInformation("No bans found for current UserId={UserId}", userIdClaim.Value);
                return NotFound();
            }
            return allByMyUserId;
        }
    }

    [HttpGet]
    public async Task<ActionResult<UserDto?>> GetUserById(string userId)
    {
        using (_logger.BeginScope("GetUserById action"))
        {
            _logger.LogInformation("GetUserById called with UserId={UserId}", userId);
            _logger.LogInformation("Fetching user for current UserId={UserId}", userId);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for UserId={UserId}", userId);
                return NotFound("User not found.");
            }

            _logger.LogInformation("Found user for current UserId={UserId}", userId);
            return user;
        }
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        using (_logger.BeginScope("GetAllUsers action"))
        {
            _logger.LogInformation("GetAllUsers called");
            var users = await _userService.GetAllUsersAsync();
            var allUsers = users as UserDto[] ?? users.ToArray();
            _logger.LogInformation("Found {Count} users", allUsers.Length);
            return allUsers;
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<UserDto?>> GetUserByMyId()
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
            return user;
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersByPages(int pageNumber, int pageSize)
    {
        using (_logger.BeginScope("GetAllUsersByPages action"))
        {
            _logger.LogInformation("GetAllUsersByPages called with PageNumber={pageNumber}, PageSize={pageSize}", pageNumber, pageSize);
            var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
            if (users == null)
            {
                _logger.LogWarning("No users found");
                return NotFound();
            }
            var any = users.ToArray();
            _logger.LogInformation("Found {Count} users", any.Length);
            return any;
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
    {
        using (_logger.BeginScope("GetUserByUsername action"))
        {
            _logger.LogInformation("GetUserByUsername called with Username={username}", username);
            var users = await _userService.GetUserByUsernameAsync(username);
            if (users == null)
            {
                _logger.LogWarning("User found");
                return NotFound();
            }
            _logger.LogInformation("Found user for current Username={username}", username);
            return users;
        }
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(string role)
    {
        using (_logger.BeginScope("GetUsersByRole action"))
        {
            _logger.LogInformation("GetUsersByRole called with Role={role}", role);
            var users = await _userService.GetUsersByRoleAsync(role);
            if (users == null)
            {
                _logger.LogWarning("No users found");
                return NotFound();
            }
            var any = users.ToArray();
            _logger.LogInformation("Found {Count} users", any.Length);
            return any;
        }
    }
    
    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRoleByPages(string role, int pageNumber, int pageSize)
    {
        using (_logger.BeginScope("GetUsersByRoleByPages action"))
        {
            _logger.LogInformation("GetUsersByRoleByPages called with Role={role}", role);
            var users = await _userService.GetUsersByRoleAsync(role, pageNumber, pageSize);
            if (users == null)
            {
                _logger.LogWarning("No users found");
                return NotFound();
            }
            var any = users.ToArray();
            _logger.LogInformation("Found {Count} users", any.Length);
            return any;
        }
    }

    // [HttpPut]
    // [Authorize]
    // public async Task<ActionResult> UpdateUser([FromBody] UserDto user)
    // {
    //     using (_logger.BeginScope("UpdateUser action"))
    //     {
    //         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //         if (userId == null)
    //         {
    //             _logger.LogWarning("UserId claim is missing");
    //             return BadRequest();
    //         }
    //         _logger.LogInformation("UpdateUser called with UserId={userId}", userId);
    //         if (user.Id != userId)
    //         {
    //             _logger.LogWarning("UserId does not match");
    //             return Forbid();
    //         }
    //         var result = await _userService.UpdateUserAsync(user);
    //         _logger.LogInformation("Updated user id={userId}", userId);
    //         return NoContent();
    //     }
    // }
}