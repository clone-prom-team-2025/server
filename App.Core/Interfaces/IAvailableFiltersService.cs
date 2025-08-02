using App.Core.DTOs.AvailableFilters;

namespace App.Core.Interfaces;

public interface IAvailableFiltersService
{
    Task CreateFilterCollectionAsync(AvailableFiltersCreateDto filters);
    Task<List<AvailableFiltersDto>> GetAllFiltersAsync();
    Task<List<AvailableFiltersDto>> GetAllFiltersAsync(string categoryId);
    Task<bool> RemoveCollectionByCategoryIdAsync(string categoryId);
    Task<bool> RemoveCollectionByIdAsync(string id);
    Task AddFilterToCollectionAsync(string categoryId, List<AvailableFiltersDto> filtersDto);
    Task<bool> RemoveFilterFromCollectionAsync(string categoryId, List<string> values);
    Task<bool> UpdateFilterCollectionAsync(string id, List<AvailableFiltersItemDto> filters);
    Task<bool> UpdateFilterCollectionAsync(AvailableFiltersDto updatedFilters);
}