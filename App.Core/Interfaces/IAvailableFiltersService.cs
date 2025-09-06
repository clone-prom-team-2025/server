using App.Core.DTOs.AvailableFilters;

namespace App.Core.Interfaces;

public interface IAvailableFiltersService
{
    Task CreateFilterCollectionAsync(AvailableFiltersCreateDto filters);
    Task<List<AvailableFiltersDto>> GetAllFiltersAsync();
    Task<List<AvailableFiltersDto>> GetAllFiltersAsync(string categoryId);
    Task RemoveCollectionByCategoryIdAsync(string categoryId);
    Task RemoveCollectionByIdAsync(string id);
    Task AddFilterToCollectionAsync(string categoryId, List<AvailableFiltersItemDto> filtersDto);
    Task RemoveFilterFromCollectionAsync(string categoryId, List<string> values);
    Task UpdateFilterCollectionAsync(string id, List<AvailableFiltersItemDto> filters);
    Task UpdateFilterCollectionAsync(AvailableFiltersDto updatedFilters);
}