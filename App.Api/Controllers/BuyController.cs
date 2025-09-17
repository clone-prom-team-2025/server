using System.Security.Claims;
using App.Core.DTOs.Sell;
using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.Sell;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class BuyController : ControllerBase
{
    private readonly ILogger<BuyController> _logger;
    private readonly IBuyService _buyService;

    public BuyController(ILogger<BuyController> logger, IBuyService buyService)
    {
        _logger = logger;
        _buyService = buyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetById(string buyId)
    {
        using (_logger.BeginScope("GetBuyId"))
        {
            _logger.LogInformation("GetBuyId called");
            var result = await _buyService.GetBuyInfoAsync(buyId);
            return Ok(result);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetByMyId()
    {
        using (_logger.BeginScope("GetByUserId"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("GetByUserId called");
            var result = await _buyService.GetBuyInfoByUserIdAsync(userId);
            return Ok(result);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetBySellerId(string sellerId)
    {
        using (_logger.BeginScope("GetBySellerId"))
        {
            _logger.LogInformation("GetBySellerId called");
            var result = await _buyService.GetBuyInfoBySellerIdAsync(sellerId);
            return Ok(result);
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> BuyProduct([FromBody] BuyCreateDto dto)
    {
        using (_logger.BeginScope("BuyProduct"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("BuyProduct action");
            await _buyService.BuyProductAsync(dto, userId);
            _logger.LogInformation("BuyProduct action completed");
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Accept(string buyId)
    {
        using (_logger.BeginScope("Accept"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("Accept action");
            await _buyService.AcceptSellAsync(buyId, userId);
            _logger.LogInformation("Accept action completed");
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Pay(string buyId)
    {
        using (_logger.BeginScope("Pay"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("Pay action");
            await _buyService.PayForProductAsync(buyId, userId);
            _logger.LogInformation("Pay action completed");
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendProduct(string buyId)
    {
        using (_logger.BeginScope("SendProduct"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("SendProduct action");
            await _buyService.SendProductAsync(buyId, userId);
            _logger.LogInformation("SendProduct action completed");
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(string buyId)
    {
        using (_logger.BeginScope("Cancel"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("Cancel action");
            await _buyService.CancelSellAsync(buyId, userId);
            _logger.LogInformation("Cancel action completed");
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Decline(string buyId)
    {
        using (_logger.BeginScope("Decline"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("Decline action");
            await _buyService.DeclineSellAsync(buyId, userId);
            _logger.LogInformation("Decline action completed");
            return NoContent();
        }
    }
}