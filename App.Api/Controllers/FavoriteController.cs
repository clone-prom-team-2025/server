using System.Security.Claims;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class FavoriteController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;
    private readonly ILogger<FavoriteController> _logger;

    public FavoriteController(IFavoriteService favoriteService, ILogger<FavoriteController> logger)
    {
        _favoriteService = favoriteService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFavoriteProducts()
    {
        using (_logger.BeginScope("GetAllFavoriteProducts"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("GetAllFavoriteProducts called with UserId={userId}", userId);
            var result = await _favoriteService.GetFavoriteProductAllByUserIdAsync(userId);
            _logger.LogInformation("GetAllFavoriteProducts successfully");
            return Ok(result);
        }
    }

    // [HttpPost]
    // public async Task<IActionResult> CreateDefaultFavoriteProductCollectionIfNotExist()
    // {
    //     using (_logger.BeginScope("CreateDefaultFavoriteProductCollectionIfNotExist"))
    //     {
    //         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //         if (userId == null)
    //             return BadRequest();
    //         _logger.LogInformation("CreateDefaultFavoriteProductCollectionIfNotExist");
    //         await _favoriteService.CreateDefaultFavoriteProductCollectionIfNotExist(userId);
    //         _logger.LogInformation("CreateDefaultFavoriteProductCollectionIfNotExist");
    //         return NoContent();
    //     }
    // }

    [HttpPost]
    public async Task<IActionResult> CreateEmptyFavoriteProductCollection(string name)
    {
        using (_logger.BeginScope("CreateEmptyFavoriteProductCollection"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("CreateEmptyFavoriteProductCollection");
            await _favoriteService.CreateEmptyFavoriteProductCollection(name, userId);
            return NoContent();
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFavoriteProductCollectionName(string id, string name)
    {
        using (_logger.BeginScope("UpdateFavoriteProductCollection action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("UpdateFavoriteProductCollectionName");
            await _favoriteService.UpdateFavoriteProductCollectionName(id, name, userId);
            _logger.LogInformation("UpdateFavoriteProductCollectionName successfully");
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddToFavoriteProductCollection(string id, string productId)
    {
        using (_logger.BeginScope("AddToFavoriteProductCollection action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("AddToFavoriteProductCollection");
            await _favoriteService.AddToFavoriteProductCollection(id, userId, productId);
            _logger.LogInformation("AddToFavoriteProductCollection successfully");
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddToFavoriteProductCollectionByName(string name, string productId)
    {
        using (_logger.BeginScope("AddToFavoriteProductCollectionByName action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("AddToFavoriteProductCollectionByName");
            await _favoriteService.AddToFavoriteProductCollectionByName(name, userId, productId);
            _logger.LogInformation("AddToFavoriteProductCollectionByName successfully");
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddToFavoriteProductCollectionToDefault(string productId)
    {
        using (_logger.BeginScope("AddToFavoriteProductCollectionByName action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("AddToFavoriteProductCollectionByName");
            await _favoriteService.AddToFavoriteProductCollectionToDefault(userId, productId);
            _logger.LogInformation("AddToFavoriteProductCollectionByName successfully");
            return NoContent();
        }
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveFromFavoriteProductCollection(string id, string productId)
    {
        using (_logger.BeginScope("RemoveFromFavoriteProductCollection action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("RemoveFromFavoriteProductCollection");
            await _favoriteService.RemoveFromFavoriteProductCollection(id, userId, productId);
            _logger.LogInformation("RemoveFromFavoriteProductCollection successfully");
            return NoContent();
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFavoriteProductCollection(string id)
    {
        using (_logger.BeginScope("DeleteFavoriteProductCollection action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("DeleteFavoriteProductCollection");
            await _favoriteService.DeleteFavoriteProductCollection(id, userId);
            _logger.LogInformation("DeleteFavoriteProductCollection successfully");
            return NoContent();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSellerProducts()
    {
        using (_logger.BeginScope("GetAllSellerProducts"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("GetAllSellerProducts called with UserId={userId}", userId);
            var result = await _favoriteService.GetFavoriteSellerAllByUserIdAsync(userId);
            _logger.LogInformation("GetAllSellerProducts successfully");
            return Ok(result);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddToFavoriteSellerCollection(string productId)
    {
        using (_logger.BeginScope("AddToFavoriteSellerCollection action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("AddToFavoriteSellerCollection");
            await _favoriteService.AddToFavoriteSellerCollectionToDefault(userId, productId);
            _logger.LogInformation("AddToFavoriteSellerCollection successfully");
            return NoContent();
        }
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveFromFavoriteSellerCollection(string id, string productId)
    {
        using (_logger.BeginScope("RemoveFromFavoriteSellerCollection action"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return BadRequest();
            _logger.LogInformation("RemoveFromFavoriteSellerCollection");
            await _favoriteService.RemoveFromFavoriteSellerCollection(id, userId, productId);
            _logger.LogInformation("RemoveFromFavoriteSellerCollection successfully");
            return NoContent();
        }
    }
}