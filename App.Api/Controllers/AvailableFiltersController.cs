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
        await _availableFiltersService.CreateFilterCollectionAsync(filters);
        return NoContent();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllFilters()
    {
        var filters = await _availableFiltersService.GetAllFiltersAsync();
        return Ok(filters);
    }

    [HttpGet("{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllFiltersByCategory(string categoryId)
    {
        var filters = await _availableFiltersService.GetAllFiltersAsync(categoryId);
        return Ok(filters);
    }

    [HttpDelete("by-category/{categoryId}")]
    public async Task<IActionResult> RemoveCollectionByCategoryId(string categoryId)
    {
        await _availableFiltersService.RemoveCollectionByCategoryIdAsync(categoryId);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveCollectionById(string id)
    {
        await _availableFiltersService.RemoveCollectionByIdAsync(id);
        return NoContent();
    }

    [HttpPut("{categoryId}/insert-filters")]
    public async Task<IActionResult> AddFilterToCollection(string categoryId,
        [FromBody] List<AvailableFiltersItemDto> filtersDto)
    {
        await _availableFiltersService.AddFilterToCollectionAsync(categoryId, filtersDto);
        return NoContent();
    }

    [HttpDelete("{categoryId}/remove-filters")]
    public async Task<IActionResult> RemoveFilterFromCollection(string categoryId, [FromBody] List<string> values)
    {
        await _availableFiltersService.RemoveFilterFromCollectionAsync(categoryId, values);
        return NoContent();
    }

    [HttpPut("{id}/update-filters")]
    public async Task<IActionResult> UpdateFilterCollectionById(string id,
        [FromBody] List<AvailableFiltersItemDto> filters)
    {
        await _availableFiltersService.UpdateFilterCollectionAsync(id, filters);
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFilterCollection([FromBody] AvailableFiltersDto updatedFilters)
    {
        await _availableFiltersService.UpdateFilterCollectionAsync(updatedFilters);
        return NoContent();
    }
}