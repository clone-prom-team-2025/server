using App.Core.DTOs;
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

    // GET: api/product
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        if (products == null || products.Count == 0)
            return NoContent();
        return Ok(products);
    }

    // GET: api/product/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(string id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();
        return Ok(product);
    }

    // POST: api/product
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ProductCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var product = new Product
        {
            Name = dto.Name,
            ProductType = dto.ProductType,
            CategoryPath = dto.CategoryPath,
            SellerId = dto.SellerId,
            Images = dto.Images
        };
        await _productService.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT: api/product/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] ProductUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();
        product.Name = dto.Name;
        product.ProductType = dto.ProductType;
        product.CategoryPath = dto.CategoryPath;
        product.Images = dto.Images;
        var updated = await _productService.UpdateAsync(product);
        if (!updated) return NotFound();
        return NoContent();
    }

    // DELETE: api/product/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
} 