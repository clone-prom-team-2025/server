using App.Core.DTOs.Categoty;
using App.Core.Models;

namespace App.Core.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>?> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(string id);
    Task<CategoryDto?> GetByNameAsync(string name);
    Task<List<CategoryDto>?> GetByParentIdAsync(string patrentId);
    Task CreateAsync(CategoryCreateDto category);
    Task UpdateAsync(CategoryDto category);
    Task DeleteAsync(string id);
    Task<List<CategoryDto>?> SearchAsync(string name);
    Task CreateManyAsync(List<CategoryCreateDto> categoryCreateDtoList);
    Task<List<CategoryNode>?> GetFullTreeAsync();
    Task<CategoryNode?> GetCategoryTreeAsync(string parentId);
    Task<List<CategoryNode>?> GetChildrenAsync(string parentId);
}