using App.Core.DTOs.AvailableFilters;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
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
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<List<AvailableFiltersDto>>> GetAllFilters()
    {
        var filters = await _availableFiltersService.GetAllFiltersAsync();
        return Ok(filters);
    }

    [HttpGet("{categoryId}")]
    public async Task<ActionResult<List<AvailableFiltersDto>>> GetAllFiltersByCategory(string categoryId)
    {
        var filters = await _availableFiltersService.GetAllFiltersAsync(categoryId);
        return Ok(filters);
    }

    [HttpDelete("by-category/{categoryId}")]
    public async Task<IActionResult> RemoveCollectionByCategoryId(string categoryId)
    {
        var result = await _availableFiltersService.RemoveCollectionByCategoryIdAsync(categoryId);
        return result ? Ok() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveCollectionById(string id)
    {
        var result = await _availableFiltersService.RemoveCollectionByIdAsync(id);
        return result ? Ok() : NotFound();
    }

    [HttpPost("{categoryId}/add-filters")]
    public async Task<IActionResult> AddFilterToCollection(string categoryId, [FromBody] List<AvailableFiltersDto> filtersDto)
    {
        await _availableFiltersService.AddFilterToCollectionAsync(categoryId, filtersDto);
        return Ok();
    }

    [HttpDelete("{categoryId}/remove-filters")]
    public async Task<IActionResult> RemoveFilterFromCollection(string categoryId, [FromBody] List<string> values)
    {
        var result = await _availableFiltersService.RemoveFilterFromCollectionAsync(categoryId, values);
        return result ? Ok() : NotFound();
    }

    [HttpPut("{id}/update-filters")]
    public async Task<IActionResult> UpdateFilterCollectionById(string id, [FromBody] List<AvailableFiltersItemDto> filters)
    {
        var result = await _availableFiltersService.UpdateFilterCollectionAsync(id, filters);
        return result ? Ok() : NotFound();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFilterCollection([FromBody] AvailableFiltersDto updatedFilters)
    {
        var result = await _availableFiltersService.UpdateFilterCollectionAsync(updatedFilters);
        return result ? Ok() : NotFound();
    }
}
