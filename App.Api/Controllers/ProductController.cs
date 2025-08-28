using App.Core.DTOs.Product;
using App.Core.Interfaces;
using App.Core.Models.Product;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<ProductFilterResponseDto?>> GetAllAsync(ProductFilterRequestDto filter)
    {
        var products = await _productService.GetAllAsync(filter);
        if (products == null || products.Products.Count() == 0)
            return NoContent();

        return Ok(products);
    }

    [HttpGet("get-by-id/{id}")]
    public async Task<ActionResult<ProductDto?>> GetByIdAsync(string id)
    {
        var product = await _productService.GetByIdAsync(id);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost("get-by-name/{name}")]
    public async Task<ActionResult<ProductFilterResponseDto?>> GetByNameAsync([FromQuery] string name,
        ProductFilterRequestDto filter)
    {
        var products = await _productService.GetByNameAsync(name, filter);
        if (products == null || products.Products.Count() == 0)
            return NotFound();
        return Ok(products);
    }

    [HttpPost("get-by-seller-id/{sellerId}")]
    public async Task<ActionResult<ProductFilterResponseDto?>> GetBySellerIdAsync([FromQuery] string sellerId,
        ProductFilterRequestDto filter)
    {
        var products = await _productService.GetBySellerIdAsync(sellerId, filter);
        if (products == null || products.Products.Count() == 0)
            return NotFound();
        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateAsync([FromBody] ProductCreateDto productDto)
    {
        return Ok(await _productService.CreateAsync(productDto));
    }

    [HttpPut]
    public async Task<ActionResult<ProductDto?>> UpdateAsync(ProductDto productDto)
    {
        var product = await _productService.UpdateAsync(productDto);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAsync(string id)
    {
        return await _productService.DeleteAsync(id) ? NoContent() : BadRequest();
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<ProductSearchResult>?>> SearchByNameAsync(string name)
    {
        var search = await _productService.SearchByNameAsync(name);
        return search == null ? NotFound() : Ok(search);
    }
}