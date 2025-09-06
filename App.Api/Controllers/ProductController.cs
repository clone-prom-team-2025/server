using System.Security.Claims;
using App.Core.Constants;
using App.Core.DTOs.Product;
using App.Core.Interfaces;
using App.Core.Models.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Exceptions;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost("get-all")]
    public async Task<IActionResult> GetAllAsync(ProductFilterRequestDto filter)
    {
        try
        {
            var products = await _productService.GetAllAsync(filter);
            if (products == null || products.Products.Count == 0)
                return NoContent();

            return Ok(products);
        }
        catch (AppException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("get-by-id/{id}")]
    public async Task<ActionResult<ProductDto?>> GetByIdAsync(string id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }
        catch (AppException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("get-by-name/{name}")]
    public async Task<ActionResult<ProductFilterResponseDto?>> GetByNameAsync(string name,
        ProductFilterRequestDto filter)
    {
        try
        {
            var products = await _productService.GetByNameAsync(name, filter);
            if (products == null || products.Products.Count == 0)
                return NotFound();
            return Ok(products);
        }
        catch (AppException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("get-by-seller-id/{sellerId}")]
    public async Task<IActionResult> GetBySellerIdAsync(string sellerId,
        ProductFilterRequestDto filter)
    {
        try
        {
            var products = await _productService.GetBySellerIdAsync(sellerId, filter);
            if (products == null || products.Products.Count == 0)
                return NotFound();
            return Ok(products);
        }
        catch (AppException e)
        {
            return BadRequest(e.Message);
        }
        
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync([FromBody] ProductCreateDto productDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest();
            }

            await _productService.CreateAsync(productDto, userId);
            return NoContent();
        }
        catch (AppException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<ProductDto?>> UpdateAsync(UpdateProductDto productDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest();
            }
            await _productService.UpdateAsync(productDto, userId);
            return NoContent();
        }
        catch (AppException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> DeleteAsync(string id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest();
            }

            await _productService.DeleteAsync(id, userId);
            return NoContent();
        }
        catch (AppException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<ProductSearchResult>?>> SearchByNameAsync(string name)
    {
        var search = await _productService.SearchByNameAsync(name);
        return search == null ? NotFound() : Ok(search);
    }
}