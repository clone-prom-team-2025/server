using App.Core.DTOs;
using App.Core.DTOs.Product;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductMediaController : ControllerBase
{
    private readonly IProductMediaService _productMediaService;

    public ProductMediaController(IProductMediaService productMediaService)
    {
        _productMediaService = productMediaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductMediaDto>>> GetAll()
    {
        var media = await _productMediaService.GetAll();
        return media == null ? NoContent() : Ok(media);
    }

    [HttpDelete("by-id/{id}")]
    public async Task<ActionResult> DeleteMediaAsync(string id)
    {
        return await _productMediaService.DeleteAsync(id) ? Ok() : NotFound();
    }

    [HttpDelete("by-product-id/{productId}")]
    public async Task<ActionResult> DeleteMediaByProductIdAsync(string productId)
    {
        return await _productMediaService.DeleteByProductIdAsync(productId) ? Ok() : NotFound();
    }

    [HttpGet("by-product-id/{productId}")]
    public async Task<ActionResult<List<ProductMediaDto>?>> GetByProdcutIdAsync(string productId)
    {
        return await _productMediaService.GetByProductIdAsync(productId);
    }

    [HttpPut("many")]
    public async Task<ActionResult<List<ProductMediaDto>?>> SyncProductMediaAsync([FromForm] IFormFile[] files,
        [FromQuery] string productId)
    {
        if (files == null || files.Length == 0)
            return BadRequest("No files uploaded.");

        var tempDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp");
        if (!Directory.Exists(tempDir))
            Directory.CreateDirectory(tempDir);

        List<FileArrayItemDto> filesDto = [];

        for (var i = 0; i < files.Length; i++)
        {
            var file = files[i];
            if (file.Length > 0)
            {
                var tempFilePath = Path.Combine(tempDir, Path.GetRandomFileName());
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                filesDto.Add(new FileArrayItemDto
                {
                    FileName = file.FileName,
                    Order = i,
                    Url = tempFilePath
                });
            }
        }

        var result = await _productMediaService.SyncMediaFromTempFilesAsync(filesDto, productId);

        foreach (var file in filesDto)
            if (!string.IsNullOrWhiteSpace(file.Url) && System.IO.File.Exists(file.Url))
                System.IO.File.Delete(file.Url);

        return Ok(result);
    }
}