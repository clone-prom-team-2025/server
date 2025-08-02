using App.Core.Models.AvailableFilters;

namespace App.Core.Interfaces;

public interface IAvailableFiltersRepository
{
    Task CreateFilterCollectionAsync(AvailableFilters filters);
    Task<List<AvailableFilters>> GetAllFiltersAsync();
    Task<List<AvailableFilters>> GetAllFiltersAsync(string categoryId);
    Task<bool> RemoveCollectionByCategoryIdAsync(string categoryId);
    Task<bool> RemoveCollectionByIdAsync(string id);
    Task AddFilterToCollectionAsync(string categoryId, List<AvailableFiltersItem> filters);
    Task<bool> RemoveFilterFromCollectionAsync(string categoryId, List<string> values);
    Task<bool> UpdateFilterCollectionAsync(string id, List<AvailableFiltersItem> filters);
    Task<bool> UpdateFilterCollectionAsync(AvailableFilters updatedFilters);
}