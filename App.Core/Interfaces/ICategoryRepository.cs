using App.Core.DTOs.Categoty;
using App.Core.Models;

namespace App.Core.Interfaces;

public interface ICategoryRepository
{
    Task<List<CategoryDto>?> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(string id);
    Task<CategoryDto?> GetByNameAsync(string name, string languageCode);
    Task<List<CategoryDto>?> GetByParentIdAsync(string patrentId);
    Task CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(string id);
    Task<List<CategoryDto>?> SearchAsync(string name, string languageCode);
    Task<CategoryNode?> GetParentsTreeAsync(string id);
    Task<CategoryNode?> GetCategoryTreeAsync(string parentId);
    Task<List<CategoryNode>?> GetChildrenAsync(string parentId);
    Task<List<CategoryNode>?> GetFullTreeAsync();
}