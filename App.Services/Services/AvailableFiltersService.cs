using App.Core.DTOs.AvailableFilters;
using App.Core.Interfaces;
using App.Core.Models.AvailableFilters;
using AutoMapper;

namespace App.Services.Services;

/// <summary>
/// Provides operations for managing available filters collections.
/// </summary>
public class AvailableFiltersService(IAvailableFiltersRepository repository, IMapper mapper) : IAvailableFiltersService
{
    private readonly IMapper _mapper = mapper;
    private readonly IAvailableFiltersRepository _repository = repository;

    /// <summary>
    /// Creates a new filter collection.
    /// </summary>
    /// <param name="filters">The filter collection to create.</param>
    public async Task CreateFilterCollectionAsync(AvailableFiltersCreateDto filters)
    {
        await _repository.CreateFilterCollectionAsync(_mapper.Map<AvailableFilters>(filters));
    }

    /// <summary>
    /// Retrieves all filter collections.
    /// </summary>
    public async Task<List<AvailableFiltersDto>> GetAllFiltersAsync()
    {
        return _mapper.Map<List<AvailableFiltersDto>>(await _repository.GetAllFiltersAsync());
    }

    /// <summary>
    /// Retrieves all filter collections by category.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    public async Task<List<AvailableFiltersDto>> GetAllFiltersAsync(string categoryId)
    {
        return _mapper.Map<List<AvailableFiltersDto>>(await _repository.GetAllFiltersAsync(categoryId));
    }

    /// <summary>
    /// Removes a filter collection by category ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    /// <exception cref="InvalidOperationException">Thrown if the collection could not be removed.</exception>
    public async Task RemoveCollectionByCategoryIdAsync(string categoryId)
    {
        if (!await _repository.RemoveCollectionByCategoryIdAsync(categoryId))
            throw new InvalidOperationException("Could not remove collection");
    }

    /// <summary>
    /// Removes a filter collection by its ID.
    /// </summary>
    /// <param name="id">The collection ID.</param>
    /// <exception cref="InvalidOperationException">Thrown if the collection could not be removed.</exception>
    public async Task RemoveCollectionByIdAsync(string id)
    {
        if (!await _repository.RemoveCollectionByIdAsync(id))
            throw new InvalidOperationException("Could not remove collection");
    }

    /// <summary>
    /// Adds filters to a collection by category ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    /// <param name="filtersDto">The filters to add.</param>
    public async Task AddFilterToCollectionAsync(string categoryId, List<AvailableFiltersItemDto> filtersDto)
    {
        await _repository.AddFilterToCollectionAsync(categoryId, _mapper.Map<List<AvailableFiltersItem>>(filtersDto));
    }

    /// <summary>
    /// Removes filters from a collection by category ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    /// <param name="values">The filter values to remove.</param>
    /// <exception cref="InvalidOperationException">Thrown if the filters could not be removed.</exception>
    public async Task RemoveFilterFromCollectionAsync(string categoryId, List<string> values)
    {
        if (!await _repository.RemoveFilterFromCollectionAsync(categoryId, values))
            throw new InvalidOperationException("Could not remove filter");
    }
    
    /// <summary>
    /// Updates a filter collection by its ID.
    /// </summary>
    /// <param name="id">The collection ID.</param>
    /// <param name="filters">The updated filters.</param>
    /// <exception cref="InvalidOperationException">Thrown if the collection could not be updated.</exception>
    public async Task UpdateFilterCollectionAsync(string id, List<AvailableFiltersItemDto> filters)
    {
        if (!await _repository.UpdateFilterCollectionAsync(id, _mapper.Map<List<AvailableFiltersItem>>(filters)))
            throw new InvalidOperationException("Could not update filter");
    }

    /// <summary>
    /// Updates a filter collection.
    /// </summary>
    /// <param name="updatedFilters">The updated filter collection.</param>
    /// <exception cref="InvalidOperationException">Thrown if the collection could not be updated.</exception>
    public async Task UpdateFilterCollectionAsync(AvailableFiltersDto updatedFilters)
    {
        if (!await _repository.UpdateFilterCollectionAsync(_mapper.Map<AvailableFilters>(updatedFilters)))
            throw new InvalidOperationException("Could not update filter");
    }
}