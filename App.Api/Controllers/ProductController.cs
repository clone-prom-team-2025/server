using System.Security.Claims;
using App.Core.DTOs.Product;
using App.Core.Interfaces;
using App.Core.Models.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService,  ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpPost("get-all")]
    public async Task<IActionResult> GetAll(ProductFilterRequestDto filter)
    {
        using (_logger.BeginScope("GetAll")){
            _logger.LogInformation("GetAll action");
            var products = await _productService.GetAllAsync(filter);
            if (products == null || products.Products.Count == 0)
                return NoContent();
            _logger.LogInformation("GetAll success");

            return Ok(products);
        }
    }

    [HttpGet("get-by-id/{id}")]
    public async Task<ActionResult<ProductDto?>> GetByIdAsync(string id)
    {
        using (_logger.BeginScope("GetById")) {
            _logger.LogInformation("GetById action");
            var product = await _productService.GetByIdAsync(id);
            _logger.LogInformation("GetById success");
            return product == null ? NotFound() : Ok(product);
        }
    }

    [HttpPost("get-by-name/{name}")]
    public async Task<ActionResult<ProductFilterResponseDto?>> GetByNameAsync(string name,
        ProductFilterRequestDto filter)
    {
        using (_logger.BeginScope("GetByName")) {
            _logger.LogInformation("GetByName action");
            var products = await _productService.GetByNameAsync(name, filter);
            if (products == null || products.Products.Count == 0)
                return NotFound();
            _logger.LogInformation("GetByName success");
            return Ok(products);
        }
    }

    [HttpPost("get-by-seller-id/{sellerId}")]
    public async Task<IActionResult> GetBySellerIdAsync(string sellerId,
        ProductFilterRequestDto filter)
    {
        using  (_logger.BeginScope("GetBySellerId")) {
            var products = await _productService.GetBySellerIdAsync(sellerId, filter);
            if (products == null || products.Products.Count == 0)
                return NotFound();
            _logger.LogInformation("GetBySellerId success");
            return Ok(products);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync([FromBody] ProductCreateDto productDto)
    {
        using (_logger.BeginScope("Create")) {
            _logger.LogInformation("Create action");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return BadRequest();

            await _productService.CreateAsync(productDto, userId);
            _logger.LogInformation("Create success");
            return NoContent();
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<ProductDto?>> UpdateAsync(UpdateProductDto productDto)
    {
        using (_logger.BeginScope("Update")) {
            _logger.LogInformation("Update action");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return BadRequest();
            await _productService.UpdateAsync(productDto, userId);
            _logger.LogInformation("Update success");
            return NoContent();
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> DeleteAsync(string id)
    {
        using (_logger.BeginScope("Delete")) {
            _logger.LogInformation("Delete action");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return BadRequest();

            await _productService.DeleteAsync(id, userId);
            _logger.LogInformation("Delete success");
            return NoContent();
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<ProductSearchResult>?>> SearchByNameAsync(string name)
    {
        using (_logger.BeginScope("SearchByName")) {
            _logger.LogInformation("SearchByNameAsync action");
            var search = await _productService.SearchByNameAsync(name);
            _logger.LogInformation("SearchByNameAsync success");
            return search == null ? NotFound() : Ok(search);
        }
    }

    [HttpGet("random")]
    public async Task<ActionResult<ProductDto?>> GetRandomAsync(int page, int pageSize)
    {
        using (_logger.BeginScope("GetRandom")) {
            _logger.LogInformation("GetRandomAsync action");
            var result = await _productService.GetRandomProductsAsync(page, pageSize);
            _logger.LogInformation("GetRandomAsync success");
            return Ok(result);
        } 
    }
}