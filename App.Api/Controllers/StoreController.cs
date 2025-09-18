using System.Security.Claims;
using App.Core.Constants;
using App.Core.DTOs.Store;
using App.Core.Enums;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class StoreController : ControllerBase
{
    private readonly ILogger<StoreController> _logger;
    private readonly IStoreService _storeService;

    public StoreController(IStoreService storeService, ILogger<StoreController> logger)
    {
        _storeService = storeService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateStore([FromForm] CreateStoreCreateRequestDto dto)
    {
        using (_logger.BeginScope("CreateStore action"))
        {
            _logger.LogInformation("CreateStore called with dto");
            var userIdClaims = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaims == null)
            {
                _logger.LogError("CreateStore user claim is null");
                return BadRequest();
            }

            var stream = dto.File.OpenReadStream();
            await _storeService.CreateRequest(dto, userIdClaims.Value, stream, dto.File.FileName);

            _logger.LogInformation("Successfully created store request by UserId={userId}", userIdClaims.Value);
            return Ok();
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteRequest(string id)
    {
        using (_logger.BeginScope("DeleteRequest action"))
        {
            _logger.LogInformation("DeleteRequest called with id={id}", id);
            await _storeService.DeleteRequest(id);

            _logger.LogInformation("Successfully deleted store request by UserId={userId}", id);
            return Ok();
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRequest(UpdateStoreCreateRequestDto dto)
    {
        using (_logger.BeginScope("UpdateRequest action"))
        {
            _logger.LogInformation("UpdateRequest called with dto");
            await _storeService.UpdateRequest(dto);

            _logger.LogInformation("Successfully updated store request for Id={id}", dto.Id);
            return Ok();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRequests()
    {
        using (_logger.BeginScope("GetAllRequests action"))
        {
            _logger.LogInformation("GetAllRequests called");
            var result = await _storeService.GetAllRequests();
            var all = result.ToArray();
            _logger.LogInformation("Found {Count} store requests", all.Length);
            if (all.Length == 0)
                return NotFound();

            return Ok(all);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequestsByUserId(string userId)
    {
        using (_logger.BeginScope("GetRequestsByUserId action"))
        {
            _logger.LogInformation("GetRequestsByUserId called");
            var result = await _storeService.GetRequestByUserId(userId);
            if (result == null)
            {
                _logger.LogError("GetRequestByUserId failed for dto={userId}", userId);
                return NotFound();
            }

            _logger.LogInformation("Successfully retrieved requests for UserId={userId}", userId);
            return Ok(result);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequestById(string requestId)
    {
        using (_logger.BeginScope("GetRequestById action"))
        {
            _logger.LogInformation("GetRequestById called");
            var result = await _storeService.GetRequestById(requestId);
            if (result == null)
            {
                _logger.LogError("GetRequestById failed for dto={requestId}", requestId);
                return NotFound();
            }

            _logger.LogInformation("Successfully retrieved request for Id={requestId}", requestId);
            return Ok(result);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequestApprovedByAdminId(string adminId)
    {
        using (_logger.BeginScope("GetRequestApprovedByAdminId action"))
        {
            _logger.LogInformation("GetRequestApprovedByAdminId called");
            var result = await _storeService.GetRequestApprovedByAdminId(adminId);
            var all = result.ToArray();
            _logger.LogInformation("Found {Count} store requests for AdminId={adminId}", all.Length, adminId);
            if (all.Length == 0) return NotFound();
            return Ok(all);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequestRejectedByAdminId(string adminId)
    {
        using (_logger.BeginScope("GetRequestRejectedByAdminId action"))
        {
            _logger.LogInformation("GetRequestRejectedByAdminId called");
            var result = await _storeService.GetRequestRejectedByAdminId(adminId);
            var all = result.ToArray();
            _logger.LogInformation("Found {Count} store requests for AdminId={adminId}", all.Length, adminId);
            if (all.Length == 0) return NotFound();
            return Ok(all);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequestByMyId()
    {
        using (_logger.BeginScope("GetRequestByMyId action"))
        {
            _logger.LogInformation("GetRequestByMyId called");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            _logger.LogInformation("Fetching store requests for current UserId={UserId}", userIdClaim.Value);
            var result = await _storeService.GetRequestByUserId(userIdClaim.Value);
            if (result == null)
            {
                _logger.LogError("GetRequestByUserId failed for dto={userId}", userIdClaim.Value);
                return NotFound();
            }

            _logger.LogInformation("Successfully retrieved request for UserId={UserId}", userIdClaim.Value);
            return Ok(result);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequestApprovedByMyId()
    {
        using (_logger.BeginScope("GetRequestApprovedByAdminId action"))
        {
            _logger.LogInformation("GetRequestApprovedByAdminId called");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            _logger.LogInformation("Fetching store requests for current UserId={userId}", userIdClaim.Value);
            var result = await _storeService.GetRequestApprovedByAdminId(userIdClaim.Value);
            var all = result.ToArray();
            _logger.LogInformation("Found {Count} store requests for AdminId={adminId}", all.Length, userIdClaim.Value);
            if (all.Length == 0) return NotFound();
            return Ok(all);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequestRejectedByMyId()
    {
        using (_logger.BeginScope("GetRequestRejectedByAdminId action"))
        {
            _logger.LogInformation("GetRequestRejectedByAdminId called");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            _logger.LogInformation("Fetching store requests for current UserId={userId}", userIdClaim.Value);
            var result = await _storeService.GetRequestRejectedByAdminId(userIdClaim.Value);
            var all = result.ToArray();
            _logger.LogInformation("Found {Count} store requests for AdminId={adminId}", all.Length, userIdClaim.Value);
            if (all.Length == 0) return NotFound();
            return Ok(all);
        }
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> ApproveRequest(string requestId)
    {
        using (_logger.BeginScope("ApproveRequest action"))
        {
            _logger.LogInformation("ApproveRequest called");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            _logger.LogInformation("Fetching store request by Id={id}", requestId);
            var storeRequest = await _storeService.GetRequestById(requestId);
            if (storeRequest == null)
            {
                _logger.LogError("Request not found for Id={id}", requestId);
                return NotFound();
            }

            await _storeService.ApproveRequest(requestId, userIdClaim.Value);

            _logger.LogInformation("Successfully approved request for Id={id}", requestId);
            return NoContent();
        }
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> RejectRequest(string requestId, string reason)
    {
        using (_logger.BeginScope("RejectRequest action"))
        {
            _logger.LogInformation("RejectRequest called");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            _logger.LogInformation("Fetching store request by Id={id}", requestId);
            var storeRequest = await _storeService.GetRequestById(requestId);
            if (storeRequest == null)
            {
                _logger.LogError("Request not found for Id={id}", requestId);
                return NotFound();
            }

            await _storeService.RejectRequest(requestId, userIdClaim.Value);

            _logger.LogInformation("Successfully rejected request for Id={id}", requestId);
            return NoContent();
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteStore(string storeId)
    {
        using (_logger.BeginScope("DeleteStore action"))
        {
            _logger.LogInformation("DeleteStore called");
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            await _storeService.DeleteStore(storeId, userIdClaim);

            _logger.LogInformation("Successfully deleted store with Id={storeId}", storeId);
            return NoContent();
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetStoreById(string storeId)
    {
        using (_logger.BeginScope("GetStoreById action"))
        {
            _logger.LogInformation("GetStoreById called");
            var result = await _storeService.GetStoreById(storeId);
            if (result == null)
            {
                _logger.LogError("GetStoreById returned null");
                return NotFound();
            }

            return Ok(result);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetStores()
    {
        using (_logger.BeginScope("GetStores"))
        {
            _logger.LogInformation("GetStores called");
            var result = await _storeService.GetStores();
            if (result == null)
            {
                _logger.LogError("GetStores returned null");
                return NotFound();
            }

            var all = result.ToArray();
            _logger.LogInformation("Found {Count} stores", all.Length);
            if (all.Length == 0) return NotFound();
            return Ok(all);
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> AddMemberToStore(string memberId, StoreRole role)
    {
        using (_logger.BeginScope("AddMemberToStore action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            _logger.LogInformation("AddMemberToStore called");
            await _storeService.AddMemberToStoreAsync(userId, memberId, role);

            _logger.LogInformation("Successfully added member to store");
            return NoContent();
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> RemoveMemberFromStore(string memberId)
    {
        using (_logger.BeginScope("RemoveMemberFromStore action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            _logger.LogInformation("RemoveMemberFromStore called");
            await _storeService.RemoveMemberFromStoreAsync(userId, memberId);

            _logger.LogInformation("Successfully removed member from store");
            return NoContent();
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetStoreMembersByMyId()
    {
        using (_logger.BeginScope("GetStoreMembers action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            _logger.LogInformation("GetStoreMembers called");
            var result = await _storeService.GetStoreMembers(userId, null);
            if (result == null)
            {
                _logger.LogError("GetStoreMembers returned null");
                return NotFound();
            }

            _logger.LogInformation("Found {Count} store members", result.ToArray().Length);
            return Ok(result);
        }
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> GetStoreMembers(string storeId)
    {
        using (_logger.BeginScope("GetStoreMembers action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            _logger.LogInformation("GetStoreMembers called");
            var result = await _storeService.GetStoreMembers(userId, storeId);
            if (result == null)
            {
                _logger.LogError("GetStoreMembers returned null");
                return NotFound();
            }

            _logger.LogInformation("Found {Count} store members", result.ToArray().Length);
            return Ok(result);
        }
    }
}