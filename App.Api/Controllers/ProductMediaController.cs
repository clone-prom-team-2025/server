using System.Security.Claims;
using App.Core.Constants;
using App.Core.DTOs;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductMediaController : ControllerBase
{
    private readonly IProductMediaService _productMediaService;
    private readonly ILogger<ProductMediaController> _logger;

    public ProductMediaController(IProductMediaService productMediaService, ILogger<ProductMediaController> logger)
    {
        _productMediaService = productMediaService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        using (_logger.BeginScope("GetAll")){
            _logger.LogInformation("GetAll action");
            var media = await _productMediaService.GetAll();
            _logger.LogInformation("GetAll success");
            return media == null ? NoContent() : Ok(media);
        }
    }

    [HttpDelete("by-id/{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteMediaAsync(string id)
    {
        using (_logger.BeginScope("DeleteMediaAsync")){
            _logger.LogInformation("DeleteMediaAsync action");
            await _productMediaService.DeleteAsync(id);
            _logger.LogInformation("DeleteMediaAsync success");
            return NoContent();
        }
    }

    [HttpDelete("by-product-id/{productId}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteMediaByProductIdAsync(string productId)
    {
        using  (_logger.BeginScope("DeleteMediaByProductIdAsync")){
            _logger.LogInformation("DeleteMediaByProductIdAsync action");
            await _productMediaService.DeleteByProductIdAsync(productId);
            _logger.LogInformation("DeleteMediaByProductIdAsync success");
            return NoContent();
        }
    }

    [HttpGet("by-product-id/{productId}")]
    public async Task<IActionResult> GetByProdcutIdAsync(string productId)
    {
        using (_logger.BeginScope("GetByProdcutIdAsync")){
            _logger.LogInformation("GetByProdcutIdAsync action");
            var result = await _productMediaService.GetByProductIdAsync(productId);
            _logger.LogInformation("GetByProdcutIdAsync success");
            return Ok(result);
        }
    }

    [HttpPut("many")]
    [Authorize]
    public async Task<IActionResult> SyncProductMediaAsync([FromForm] IFormFile[] files,
        [FromQuery] string productId)
    {
        using (_logger.BeginScope("SyncProductMediaAsync"))
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("SyncProductMediaAsync action");

            if (files.Length == 0)
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

            try
            {
                var result = await _productMediaService.SyncMediaFromTempFilesAsync(filesDto, productId, userId);
                _logger.LogInformation("SyncProductMediaAsync success");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SyncProductMediaAsync");
                return BadRequest(ex.Message);
            }
            finally
            {
                foreach (var file in filesDto)
                {
                    if (!string.IsNullOrWhiteSpace(file.Url) && System.IO.File.Exists(file.Url))
                    {
                        try
                        {
                            System.IO.File.Delete(file.Url);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogWarning(deleteEx, $"Failed to delete temp file {file.Url}");
                        }
                    }
                }
            }
        }
    }
}