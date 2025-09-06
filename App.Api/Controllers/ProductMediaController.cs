using App.Core.Constants;
using App.Core.DTOs;
using App.Core.DTOs.Product;
using App.Services;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var media = await _productMediaService.GetAll();
            return media == null ? NoContent() : Ok(media);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("by-id/{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteMediaAsync(string id)
    {
        try
        {
            await _productMediaService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("by-product-id/{productId}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteMediaByProductIdAsync(string productId)
    {
        try
        {
            await _productMediaService.DeleteByProductIdAsync(productId);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("by-product-id/{productId}")]
    public async Task<IActionResult> GetByProdcutIdAsync(string productId)
    {
        try
        {
            return Ok(await _productMediaService.GetByProductIdAsync(productId));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("many")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> SyncProductMediaAsync([FromForm] IFormFile[] files,
        [FromQuery] string productId)
    {
        try
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
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }   
    }
}