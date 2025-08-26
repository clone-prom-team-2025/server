using System.Security.Claims;
using App.Core.Constants;
using App.Core.DTOs.Cart;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace App.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<ActionResult> AddToCart([FromForm] CreateCartDto dto)
    {
        using (_logger.BeginScope("AddToCartAsync action"))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogError("UserId not found");
                return BadRequest();
            }
            _logger.LogInformation("AddToCartAsync called with Dto by UserId={userId}", userIdClaim.Value);
            
            var result = await _cartService.AddAsync(dto, userIdClaim.Value);
            if (!result)
            {
                _logger.LogError("Failed to add to Cart");
                return BadRequest();
            }
            
            _logger.LogInformation("ProductId={ProdcutId} add to cart UserId={UserId}", dto.ProductId, userIdClaim.Value);
            return Ok();
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteFromCart(string id)
    {
        using (_logger.BeginScope("DeleteFromCart action"))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogError("UserId not found");
                return BadRequest();
            }
            _logger.LogInformation("DeleteFromCart called with Dto by UserId={userId}", userIdClaim.Value);
            
            var result = await _cartService.RemoveAsync(id, userIdClaim.Value);
            if (!result)
            {
                _logger.LogError("Failed to remove Cart");
                return BadRequest();
            }
            
            _logger.LogInformation("Cart={Id} deleted cart from by UserId={UserId}", id, userIdClaim.Value);
            return Ok();
        }
    }
    
    [HttpDelete]
    public async Task<ActionResult> ClearCartList()
    {
        using (_logger.BeginScope("ClearCartList action"))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogError("UserId not found");
                return BadRequest();
            }
            _logger.LogInformation("ClearCartList called with Dto by UserId={userId}", userIdClaim.Value);
            
            var result = await _cartService.ClearAsync(userIdClaim.Value);
            if (!result)
            {
                _logger.LogError("Failed to clear cart list");
                return NotFound();
            }
            
            _logger.LogInformation("Failed to clear cart list with UserId={UserId}", userIdClaim.Value);
            return Ok();
        }
    }
    
    [HttpPut]
    public async Task<ActionResult> ChangeCartPcs(string id, int pcs)
    {
        using (_logger.BeginScope("ChangeCartPcs action"))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogError("UserId not found");
                return BadRequest();
            }
            _logger.LogInformation("ChangeCartPcs called with Dto by UserId={userId}", userIdClaim.Value);
            
            var result = await _cartService.ChangePcsAsync(id, pcs, userIdClaim.Value);
            if (!result)
            {
                _logger.LogError("Failed to add to Cart");
                return BadRequest();
            }
            
            _logger.LogInformation("Cart={Id} changed for UserId={UserId}", id, userIdClaim.Value);
            return Ok();
        }
    }
    
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<IEnumerable<CartDto>>> GetAllCarts()
    {
        using (_logger.BeginScope("GetAllCarts action"))
        {
            _logger.LogInformation("GetAllCarts called");
            
            var result = await _cartService.GetAllAsync();
            if (result == null)
            {
                _logger.LogError("GetAllCarts returned null");
                return NotFound();
            }
            var all = result.ToArray();
            if (all.Length == 0)
            {
                _logger.LogInformation("GetAllCarts returned empty array");
                return NotFound();
            }
            
            _logger.LogInformation("GetAllCarts returned {Count}", all.Length);
            return all;
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetById(string id)
    {
        using (_logger.BeginScope("GetById action"))
        {
            _logger.LogInformation("GetById called");
            
            var result = await _cartService.GetByIdAsync(id);
            if (result == null)
            {
                _logger.LogError("GetById not found cart with Id={Id}", id);
                return NotFound();
            }
            _logger.LogInformation("GetById returned cart with Id={Id}", id);
            return result;
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartDto>>> GetByUserId(string userId)
    {
        using (_logger.BeginScope("GetByUserId action"))
        {
            _logger.LogInformation("GetByUserId called with UserId={UserId}", userId);
            
            var result = await _cartService.GetByUserIdAsync(userId);
            if (result == null)
            {
                _logger.LogError("GetByUserId returned null");
                return NotFound();
            }
            var all = result.ToArray();
            if (all.Length == 0)
            {
                _logger.LogInformation("GetByUserId returned empty array");
                return NotFound();
            }
            
            _logger.LogInformation("GetByUserId returned {Count}", all.Length);
            return all;
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartDto>>> GetByMyId()
    {
        using (_logger.BeginScope("GetByUserId action"))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogError("UserId not found");
                return BadRequest();
            }
            
            _logger.LogInformation("GetByUserId called with UserId={UserId}", userIdClaim.Value);
            
            var result = await _cartService.GetByUserIdAsync(userIdClaim.Value);
            if (result == null)
            {
                _logger.LogError("GetByUserId returned null");
                return NotFound();
            }
            var all = result.ToArray();
            if (all.Length == 0)
            {
                _logger.LogInformation("GetByUserId returned empty array");
                return NotFound();
            }
            
            _logger.LogInformation("GetByUserId returned {Count}", all.Length);
            return all;
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<bool>> IsProductInCart(string productId)
    {
        using (_logger.BeginScope("IsProductInCart action"))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogError("UserId not found");
                return BadRequest();
            }
            
            _logger.LogInformation("IsProductInCart called with UserId={UserId}", userIdClaim.Value);
            
            var result = await _cartService.IsProductInCartAsync(productId, userIdClaim.Value);
            _logger.LogInformation("IsProductInCart returned: {result}", result);
            return result;
        }
    }
}