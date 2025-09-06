using App.Core.Constants;
using App.Core.DTOs.AvailableFilters;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
[Route("api/[controller]")]
public class AvailableFiltersController : ControllerBase
{
    private readonly IAvailableFiltersService _availableFiltersService;

    public AvailableFiltersController(IAvailableFiltersService availableFiltersService)
    {
        _availableFiltersService = availableFiltersService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateFilterCollection([FromBody] AvailableFiltersCreateDto filters)
    {
        try
        {
            await _availableFiltersService.CreateFilterCollectionAsync(filters);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllFilters()
    {
        try
        {
            var filters = await _availableFiltersService.GetAllFiltersAsync();
            return Ok(filters);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllFiltersByCategory(string categoryId)
    {
        try
        {
            var filters = await _availableFiltersService.GetAllFiltersAsync(categoryId);
            return Ok(filters);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("by-category/{categoryId}")]
    public async Task<IActionResult> RemoveCollectionByCategoryId(string categoryId)
    {
        try
        {
            await _availableFiltersService.RemoveCollectionByCategoryIdAsync(categoryId);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveCollectionById(string id)
    {
        try
        {
            await _availableFiltersService.RemoveCollectionByIdAsync(id);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("{categoryId}/insert-filters")]
    public async Task<IActionResult> AddFilterToCollection(string categoryId,
        [FromBody] List<AvailableFiltersItemDto> filtersDto)
    {
        try
        {
            await _availableFiltersService.AddFilterToCollectionAsync(categoryId, filtersDto);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{categoryId}/remove-filters")]
    public async Task<IActionResult> RemoveFilterFromCollection(string categoryId, [FromBody] List<string> values)
    {
        try
        {
            await _availableFiltersService.RemoveFilterFromCollectionAsync(categoryId, values);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("{id}/update-filters")]
    public async Task<IActionResult> UpdateFilterCollectionById(string id,
        [FromBody] List<AvailableFiltersItemDto> filters)
    {
        try
        {
            await _availableFiltersService.UpdateFilterCollectionAsync(id, filters);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFilterCollection([FromBody] AvailableFiltersDto updatedFilters)
    {
        try
        {
            await _availableFiltersService.UpdateFilterCollectionAsync(updatedFilters);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}