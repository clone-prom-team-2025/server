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
    private readonly ILogger<AvailableFiltersController> _logger;

    public AvailableFiltersController(IAvailableFiltersService availableFiltersService, ILogger<AvailableFiltersController> logger)
    {
        _availableFiltersService = availableFiltersService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateFilterCollection([FromBody] AvailableFiltersCreateDto filters)
    {
        using (_logger.BeginScope("CreateFilterCollection")){
            _logger.LogInformation("CreateFilterCollection action");
            await _availableFiltersService.CreateFilterCollectionAsync(filters);
            _logger.LogInformation("CreateFilterCollection success");
            return NoContent();
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllFilters()
    {
        using (_logger.BeginScope("GetAllFilters")){
            _logger.LogInformation("GetAllFilters action");
            var filters = await _availableFiltersService.GetAllFiltersAsync();
            _logger.LogInformation("GetAllFilters success");
            return Ok(filters);
        }
    }

    [HttpGet("{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllFiltersByCategory(string categoryId)
    {
        using (_logger.BeginScope("GetAllFiltersByCategory")){
            _logger.LogInformation("GetAllFiltersByCategory action");
            var filters = await _availableFiltersService.GetAllFiltersAsync(categoryId);
            _logger.LogInformation("GetAllFiltersByCategory success");
            return Ok(filters);
        }
    }

    [HttpDelete("by-category/{categoryId}")]
    public async Task<IActionResult> RemoveCollectionByCategoryId(string categoryId)
    {
        using  (_logger.BeginScope("RemoveCollectionByCategoryId")){
            _logger.LogInformation("RemoveCollectionByCategoryId action");
            await _availableFiltersService.RemoveCollectionByCategoryIdAsync(categoryId);
            _logger.LogInformation("RemoveCollectionByCategoryId success");
            return NoContent();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveCollectionById(string id)
    {
        using (_logger.BeginScope("RemoveCollectionById")){
            _logger.LogInformation("RemoveCollectionById action");
            await _availableFiltersService.RemoveCollectionByIdAsync(id);
            _logger.LogInformation("RemoveCollectionById success");
            return NoContent();
        }
    }

    [HttpPut("{categoryId}/insert-filters")]
    public async Task<IActionResult> AddFilterToCollection(string categoryId,
        [FromBody] List<AvailableFiltersItemDto> filtersDto)
    {
        using (_logger.BeginScope("AddFilterToCollection")){
            _logger.LogInformation("AddFilterToCollection action");
            await _availableFiltersService.AddFilterToCollectionAsync(categoryId, filtersDto);
            _logger.LogInformation("AddFilterToCollection success");
            return NoContent();
        }
    }

    [HttpDelete("{categoryId}/remove-filters")]
    public async Task<IActionResult> RemoveFilterFromCollection(string categoryId, [FromBody] List<string> values)
    {
        using  (_logger.BeginScope("RemoveFilterFromCollection")){
            _logger.LogInformation("RemoveFilterFromCollection action");
            await _availableFiltersService.RemoveFilterFromCollectionAsync(categoryId, values);
            _logger.LogInformation("RemoveFilterFromCollection success");
            return NoContent();
        }
    }

    [HttpPut("{id}/update-filters")]
    public async Task<IActionResult> UpdateFilterCollectionById(string id,
        [FromBody] List<AvailableFiltersItemDto> filters)
    {
        using (_logger.BeginScope("UpdateFilterCollectionById")){
            _logger.LogInformation("UpdateFilterCollectionById action");
            await _availableFiltersService.UpdateFilterCollectionAsync(id, filters);
            _logger.LogInformation("UpdateFilterCollectionById success");
            return NoContent();
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFilterCollection([FromBody] AvailableFiltersDto updatedFilters)
    {
        using (_logger.BeginScope("UpdateFilterCollection")){
            _logger.LogInformation("UpdateFilterCollection action");
            await _availableFiltersService.UpdateFilterCollectionAsync(updatedFilters);
            _logger.LogInformation("UpdateFilterCollection success");
            return NoContent();
        }
    }
}