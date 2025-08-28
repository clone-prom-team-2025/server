using App.Core.DTOs.Categoty;
using App.Core.Models;

namespace App.Core.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>?> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(string id);
    Task<CategoryDto?> GetByNameAsync(string name);
    Task<List<CategoryDto>?> GetByParentIdAsync(string patrentId);
    Task<CategoryDto> CreateAsync(CategoryCreateDto category);
    Task<CategoryDto?> UpdateAsync(CategoryDto category);
    Task<bool> DeleteAsync(string id);
    Task<List<CategoryDto>?> SearchAsync(string name);
    Task<List<CategoryNode>?> GetFullTreeAsync();
    Task<CategoryNode?> GetCategoryTreeAsync(string parentId);
    Task<List<CategoryNode>?> GetChildrenAsync(string parentId);
}