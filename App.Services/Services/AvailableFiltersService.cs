using App.Core.DTOs.AvailableFilters;
using App.Core.Interfaces;
using App.Core.Models.AvailableFilters;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace App.Services.Services;

/// <summary>
///     Provides operations for managing available filters collections.
/// </summary>
public class AvailableFiltersService(IAvailableFiltersRepository repository, IMapper mapper, ILogger<AvailableFiltersService> logger) : IAvailableFiltersService
{
    private readonly IMapper _mapper = mapper;
    private readonly IAvailableFiltersRepository _repository = repository;
    private readonly ILogger<AvailableFiltersService> _logger = logger;

    /// <summary>
    ///     Creates a new filter collection.
    /// </summary>
    /// <param name="filters">The filter collection to create.</param>
    public async Task CreateFilterCollectionAsync(AvailableFiltersCreateDto filters)
    {
        _logger.LogInformation("CreateFilterCollectionAsync called for filters={@filters}", filters);
        await _repository.CreateFilterCollectionAsync(_mapper.Map<AvailableFilters>(filters));
        _logger.LogInformation("CreateFilterCollectionAsync success");
    }

    /// <summary>
    ///     Retrieves all filter collections.
    /// </summary>
    public async Task<List<AvailableFiltersDto>> GetAllFiltersAsync()
    {
        _logger.LogInformation("GetAllFiltersAsync called");
        var result = _mapper.Map<List<AvailableFiltersDto>>(await _repository.GetAllFiltersAsync());
        _logger.LogInformation("GetAllFiltersAsync success");
        return result;
    }

    /// <summary>
    ///     Retrieves all filter collections by category.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    public async Task<List<AvailableFiltersDto>> GetAllFiltersAsync(string categoryId)
    {
        _logger.LogInformation("GetAllFiltersAsync called for categoryId={CategoryId}", categoryId);
        var result = _mapper.Map<List<AvailableFiltersDto>>(await _repository.GetAllFiltersAsync(categoryId));
        _logger.LogInformation("GetAllFiltersAsync success");
        return result;
    }

    /// <summary>
    ///     Removes a filter collection by category ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    /// <exception cref="InvalidOperationException">Thrown if the collection could not be removed.</exception>
    public async Task RemoveCollectionByCategoryIdAsync(string categoryId)
    {
        _logger.LogInformation("RemoveCollectionByCategoryIdAsync called for categoryId={CategoryId}", categoryId);
        if (!await _repository.RemoveCollectionByCategoryIdAsync(categoryId))
        {
            _logger.LogWarning("RemoveCollectionByCategoryIdAsync failed for categoryId={CategoryId}", categoryId);
            throw new InvalidOperationException("Could not remove collection");
        }
        _logger.LogInformation("RemoveCollectionByCategoryIdAsync success");
    }

    /// <summary>
    ///     Removes a filter collection by its ID.
    /// </summary>
    /// <param name="id">The collection ID.</param>
    /// <exception cref="InvalidOperationException">Thrown if the collection could not be removed.</exception>
    public async Task RemoveCollectionByIdAsync(string id)
    {
        _logger.LogInformation("RemoveCollectionByIdAsync called for id={Id}", id);
        if (!await _repository.RemoveCollectionByIdAsync(id))
        {
            _logger.LogWarning("RemoveCollectionByIdAsync failed for id={Id}", id);
            throw new InvalidOperationException("Could not remove collection");
        }
        _logger.LogInformation("RemoveCollectionByIdAsync success");
    }

    /// <summary>
    ///     Adds filters to a collection by category ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    /// <param name="filtersDto">The filters to add.</param>
    public async Task AddFilterToCollectionAsync(string categoryId, List<AvailableFiltersItemDto> filtersDto)
    {
        _logger.LogInformation("AddFilterToCollectionAsync called for categoryId={CategoryId}, filtersDto={@FiltersDto}", categoryId, filtersDto);
        await _repository.AddFilterToCollectionAsync(categoryId, _mapper.Map<List<AvailableFiltersItem>>(filtersDto));
        _logger.LogInformation("AddFilterToCollectionAsync success");
    }

    /// <summary>
    ///     Removes filters from a collection by category ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    /// <param name="values">The filter values to remove.</param>
    /// <exception cref="InvalidOperationException">Thrown if the filters could not be removed.</exception>
    public async Task RemoveFilterFromCollectionAsync(string categoryId, List<string> values)
    {
        _logger.LogInformation("RemoveFilterFromCollectionAsync called for categoryId={CategoryId}, values={@Values}", categoryId, values);
        if (!await _repository.RemoveFilterFromCollectionAsync(categoryId, values))
        {
            _logger.LogWarning("RemoveFilterFromCollectionAsync failed for categoryId={CategoryId}, values={@Values}", categoryId, values);
            throw new InvalidOperationException("Could not remove filter");
        }
        _logger.LogInformation("RemoveFilterFromCollectionAsync success");
    }

    /// <summary>
    ///     Updates a filter collection by its ID.
    /// </summary>
    /// <param name="id">The collection ID.</param>
    /// <param name="filters">The updated filters.</param>
    /// <exception cref="InvalidOperationException">Thrown if the collection could not be updated.</exception>
    public async Task UpdateFilterCollectionAsync(string id, List<AvailableFiltersItemDto> filters)
    {
        _logger.LogInformation("UpdateFilterCollectionAsync called for id={Id}, filters={@Filters}", id, filters);
        if (!await _repository.UpdateFilterCollectionAsync(id, _mapper.Map<List<AvailableFiltersItem>>(filters)))
        {
            _logger.LogWarning("UpdateFilterCollectionAsync failed for id={Id}, filters={@Filters}", id, filters);
            throw new InvalidOperationException("Could not update filter");
        }
        _logger.LogInformation("UpdateFilterCollectionAsync success");
    }

    /// <summary>
    ///     Updates a filter collection.
    /// </summary>
    /// <param name="updatedFilters">The updated filter collection.</param>
    /// <exception cref="InvalidOperationException">Thrown if the collection could not be updated.</exception>
    public async Task UpdateFilterCollectionAsync(AvailableFiltersDto updatedFilters)
    {
        _logger.LogInformation("UpdateFilterCollectionAsync called for updatedFilters={@UpdatedFilters}", updatedFilters);
        if (!await _repository.UpdateFilterCollectionAsync(_mapper.Map<AvailableFilters>(updatedFilters)))
        {
            _logger.LogWarning("UpdateFilterCollectionAsync failed for updatedFilters={@UpdatedFilters}", updatedFilters);
            throw new InvalidOperationException("Could not update filter");
        }
        _logger.LogInformation("UpdateFilterCollectionAsync success");
    }
}