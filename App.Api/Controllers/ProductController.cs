using App.Core.DTOs.Product;
using App.Core.Interfaces;
using App.Core.Models.Product;
using App.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

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
    public async Task<ActionResult<List<ProductDto>?>> GetAllAsync(ProductFilterRequest filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var products = await _productService.GetAllAsync(filter);
        if (products == null || products.Count == 0)
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
    public async Task<ActionResult<List<ProductDto>?>> GetByNameAsync([FromQuery]string name, ProductFilterRequest filter)
    {
        var products = await _productService.GetByNameAsync(name, filter);
        if (products == null || products.Count == 0)
            return NotFound();
        return Ok(products);
    }

    [HttpPost("get-by-category-id/{categoryId}")]
    public async Task<ActionResult<List<ProductDto>?>> GetByCategoryAsync([FromQuery]string categoryId, ProductFilterRequest filter)
    {
        var products = await _productService.GetByCategoryAsync(categoryId, filter);
        if (products == null || products.Count == 0)
            return NotFound();
        return Ok(products);
    }

    [HttpPost("get-by-seller-id/{sellerId}")]
    public async Task<ActionResult<List<ProductDto>?>> GetBySellerIdAsync([FromQuery]string sellerId, ProductFilterRequest filter)
    {
        var products = await _productService.GetBySellerIdAsync(sellerId, filter);
        if (products == null || products.Count == 0)
            return NotFound();
        return Ok(products);
    }

    [HttpPost("get-by-model-id/{modelId}")]
    public async Task<ActionResult<ProductDto?>> GetByModelIdAsync([FromQuery]string modelId, ProductFilterRequest filter)
    {
        var product = await _productService.GetByModelIdAsync(modelId, filter);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost("get-by-model-id-many")]
    public async Task<ActionResult<List<ProductDto>?>> GetByModelIdsAsync([FromQuery] List<string> modelId, [FromBody] ProductFilterRequest filter)
    {
        var products = await _productService.GetByModelIdsAsync(modelId, filter);
        if (products == null || products.Count == 0)
            return NotFound();
        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateAsync(ProductCreateDto productDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        return Ok(await _productService.CreateAsync(productDto));
    }

    [HttpPut]
    public async Task<ActionResult<ProductDto?>> UpdateAsync(ProductDto productDto)
    {
        if (!ModelState.IsValid) return BadRequest(productDto);

        var product = await _productService.UpdateAsync(productDto);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAsync(string id)
    {
        return await _productService.DeleteAsync(id) ? NoContent() : BadRequest();
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<ProductSearchResult>?>> SearchByNameAsync(string name, string languageCode = "en")
    {
        var search = await _productService.SearchByNameAsync(name, languageCode);
        return search == null ? NotFound() : Ok(search);
    }
}