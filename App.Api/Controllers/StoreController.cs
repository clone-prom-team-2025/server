using System.Security.Claims;
using App.Core.Constants;
using App.Core.DTOs.Store;
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
    public async Task<ActionResult> CreateStore([FromForm] CreateStoreCreateRequestDto dto)
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
            var result = await _storeService.CreateRequest(dto, userIdClaims.Value, stream, dto.File.FileName);
            if (!result)
            {
                _logger.LogError("CreateStore failed for dto={dto}", dto);
                return BadRequest();
            }

            _logger.LogInformation("Successfully created store request by UserId={userId}", userIdClaims.Value);
            return Ok();
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteRequest(string id)
    {
        using (_logger.BeginScope("DeleteRequest action"))
        {
            _logger.LogInformation("DeleteRequest called with id={id}", id);
            var result = await _storeService.DeleteRequest(id);
            if (!result)
            {
                _logger.LogError("DeleteRequest failed for dto={id}", id);
                return BadRequest();
            }

            _logger.LogInformation("Successfully deleted store request by UserId={userId}", id);
            return Ok();
        }
    }

    [HttpPut]
    public async Task<ActionResult> UpdateRequest(UpdateStoreCreateRequestDto dto)
    {
        using (_logger.BeginScope("UpdateRequest action"))
        {
            _logger.LogInformation("UpdateRequest called with dto");
            var result = await _storeService.UpdateRequest(dto);
            if (!result)
            {
                _logger.LogError("UpdateRequest failed for dto={dto}", dto);
                return BadRequest();
            }

            _logger.LogInformation("Successfully updated store request for Id={id}", dto.Id);
            return Ok();
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreCreateRequestDto>>> GetAllRequests()
    {
        using (_logger.BeginScope("GetAllRequests action"))
        {
            _logger.LogInformation("GetAllRequests called");
            var result = await _storeService.GetAllRequests();
            var all = result.ToArray();
            _logger.LogInformation("Found {Count} store requests", all.Length);
            if (all.Length == 0)
                return NotFound();

            return all;
        }
    }

    [HttpGet]
    public async Task<ActionResult<StoreCreateRequestDto>> GetRequestsByUserId(string userId)
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
            return result;
        }
    }

    [HttpGet]
    public async Task<ActionResult<StoreCreateRequestDto>> GetRequestById(string requestId)
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
            return result;
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreCreateRequestDto>>> GetRequestApprovedByAdminId(string adminId)
    {
        using (_logger.BeginScope("GetRequestApprovedByAdminId action"))
        {
            _logger.LogInformation("GetRequestApprovedByAdminId called");
            var result = await _storeService.GetRequestApprovedByAdminId(adminId);
            var all = result.ToArray();
            _logger.LogInformation("Found {Count} store requests for AdminId={adminId}", all.Length, adminId);
            if (all.Length == 0) return NotFound();
            return all;
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreCreateRequestDto>>> GetRequestRejectedByAdminId(string adminId)
    {
        using (_logger.BeginScope("GetRequestRejectedByAdminId action"))
        {
            _logger.LogInformation("GetRequestRejectedByAdminId called");
            var result = await _storeService.GetRequestRejectedByAdminId(adminId);
            var all = result.ToArray();
            _logger.LogInformation("Found {Count} store requests for AdminId={adminId}", all.Length, adminId);
            if (all.Length == 0) return NotFound();
            return all;
        }
    }

    [HttpGet]
    public async Task<ActionResult<StoreCreateRequestDto>> GetRequestByMyId()
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
            return result;
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreCreateRequestDto>>> GetRequestApprovedByMyId()
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
            return all;
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreCreateRequestDto>>> GetRequestRejectedByMyId()
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
            return all;
        }
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> ApproveRequest(string requestId)
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

            var result = await _storeService.ApproveRequest(requestId, userIdClaim.Value);
            if (!result)
            {
                _logger.LogError("Failed to approve request for Id={id}", requestId);
                return BadRequest();
            }

            _logger.LogInformation("Successfully approved request for Id={id}", requestId);
            return NoContent();
        }
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> RejectRequest(string requestId, string reason)
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

            var result = await _storeService.RejectRequest(requestId, userIdClaim.Value);
            if (!result)
            {
                _logger.LogError("Failed to reject request for Id={id}", requestId);
                return BadRequest();
            }

            _logger.LogInformation("Successfully rejected request for Id={id}", requestId);
            return NoContent();
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteStore(string storeId)
    {
        using (_logger.BeginScope("DeleteStore action"))
        {
            _logger.LogInformation("DeleteStore called");
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if  (userIdClaim == null)
            {
                _logger.LogWarning("UserId claim is missing for current user");
                return BadRequest();
            }

            var result = await _storeService.DeleteStore(storeId, userIdClaim);
            if (!result)
            {
                _logger.LogError("Failed to delete store with Id={storeId}", storeId);
                return BadRequest();
            }
            _logger.LogInformation("Successfully deleted store with Id={storeId}", storeId);
            return NoContent();
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<StoreDto>> GetStoreById(string storeId)
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
            return result;
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<StoreDto>>> GetStores()
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
            return all;
        }
    }
}