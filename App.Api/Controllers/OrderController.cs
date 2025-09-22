using System.Security.Claims;
using App.Core.DTOs.Sell;
using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.Sell;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly IOrderService _orderService;

    public OrderController(ILogger<OrderController> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> BuyRegistered(DeliveryPayment deliveryPayment,
        PointsOfDelivery deliveryTo,
        string? phoneNumber, string? firstName, string? lastName, string? middleName)
    {
        using (_logger.BeginScope("BuyRegistered"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("BuyRegistered action");
            await _orderService.BuyRegistered(userId, deliveryPayment, deliveryTo, phoneNumber, firstName, lastName,
                middleName);
            _logger.LogInformation("BuyRegistered success");
            return NoContent();
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetByUserId()
    {
        using (_logger.BeginScope("GetByUserId"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("GetByUserId action");
            var result = await _orderService.GetByUserId(userId);
            _logger.LogInformation("GetByUserId success");
            return Ok(result);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetByStoreNeedToAccept()
    {
        using (_logger.BeginScope("GetByStoreNeedToAccept"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("GetByStoreNeedToAccept action");
            var result = await _orderService.GetByStoreNeedToAccept(userId);
            _logger.LogInformation("GetByStoreNeedToAccept success");
            return Ok(result);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetByStoreAccepted()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return BadRequest();
        _logger.LogInformation("GetByStoreAccepted action");
        var result = await _orderService.GetByStoreAccepted(userId);
        _logger.LogInformation("GetByStoreAccepted success");
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AcceptOrder(string orderId)
    {
        using var scope = _logger.BeginScope("AcceptOrder");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return BadRequest();
        _logger.LogInformation("AcceptOrder action");
        await _orderService.AcceptOrder(userId, orderId);
        _logger.LogInformation("AcceptOrder success");
        return NoContent();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RejectOrder(string orderId, string reason)
    {
        using var scope = _logger.BeginScope("RejectOrder");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return BadRequest();
        _logger.LogInformation("RejectOrder action");
        await _orderService.RejectOrder(userId, orderId, reason);
        _logger.LogInformation("RejectOrder success");
        return NoContent();
    }
}