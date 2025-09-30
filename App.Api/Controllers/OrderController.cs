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

    [HttpPost]
    public async Task<IActionResult> BuyUnRegistered([FromBody] BuyUnRegisteredRequest buyUnRegisteredRequest)
    {
        using (_logger.BeginScope("BuyUnRegistered"))
        {
            _logger.LogInformation("BuyUnRegistered action");
            await _orderService.BuyUnRegistered(buyUnRegisteredRequest.Products, buyUnRegisteredRequest.DeliveryPayment,
                buyUnRegisteredRequest.DeliveryTo, buyUnRegisteredRequest.Email,
                buyUnRegisteredRequest.PhoneNumber, buyUnRegisteredRequest.FirstName, buyUnRegisteredRequest.LastName,
                buyUnRegisteredRequest.MiddleName);
            _logger.LogInformation("BuyUnRegistered success");
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
    public async Task<IActionResult> GetByUserIdGrouped()
    {
        using (_logger.BeginScope("GetByUserIdGrouped"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("GetByUserIdGrouped action");
            var result = await _orderService.GetByUserIdGrouped(userId);
            _logger.LogInformation("GetByUserIdGrouped success");
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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetDeliveryAndPaymentOptions()
    {
        using var scope = _logger.BeginScope("GetDeliveryAndPaymentOptions");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return BadRequest();
        _logger.LogInformation("GetDeliveryAndPaymentOptions action");
        var result = await _orderService.GetDeliveryTypeAsync(userId);
        _logger.LogInformation("GetDeliveryAndPaymentOptions success");
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CancelOrder(string orderId)
    {
        using var scope = _logger.BeginScope("CancelOrder");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return BadRequest();
        _logger.LogInformation("CancelOrder action");
        await _orderService.CancelOrder(userId, orderId);
        _logger.LogInformation("CancelOrder success");
        return NoContent();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CancelOrdersByOrderNumber(string orderNumber)
    {
        using var scope = _logger.BeginScope("CancelOrdersByOrderNumber");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return BadRequest();
        _logger.LogInformation("CancelOrdersByOrderNumber action");
        await _orderService.CancelOrdersByOrderNumber(userId, orderNumber);
        _logger.LogInformation("CancelOrdersByOrderNumber success");
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> SendOrderActionCode(string email)
    {
        using var scope = _logger.BeginScope("SendGetOrdersByEmail");
        _logger.LogInformation("SendGetOrdersByEmail action");
        await _orderService.SendOrderActionCode(email);
        _logger.LogInformation("SendGetOrdersByEmail success");
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetByEmailCode(string email, string inputCode)
    {
        using var scope = _logger.BeginScope("GetByEmailCode");
        _logger.LogInformation("GetByEmailCode action");
        var result = await _orderService.GetByEmailCode(email, inputCode);
        _logger.LogInformation("GetByEmailCode success");
        return Ok(result);
    }


    [HttpPost]
    public async Task<IActionResult> CancelOrderByEmail(string email, string inputCode, string orderId)
    {
        using var scope = _logger.BeginScope("CancelOrderByEmail");
        _logger.LogInformation("CancelOrderByEmail action");
        await _orderService.CancelOrderByEmail(email, inputCode, orderId);
        _logger.LogInformation("CancelOrderByEmail success");
        return NoContent();
    }
}