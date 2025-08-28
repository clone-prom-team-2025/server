using App.Core.DTOs.Categoty;
using App.Core.Models;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface ICategoryRepository
{
    Task<List<CategoryDto>?> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(string id);
    Task<CategoryDto?> GetByNameAsync(string name);
    Task<List<CategoryDto>?> GetByParentIdAsync(string patrentId);
    Task CreateManyAsync(List<Category> categoryList);
    Task CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(string id);
    Task<List<CategoryDto>?> SearchAsync(string name);
    Task<CategoryNode?> GetParentsTreeAsync(string id);
    Task<CategoryNode?> GetCategoryTreeAsync(string parentId);
    Task<List<CategoryNode>?> GetChildrenAsync(string parentId);
    Task<List<CategoryNode>?> GetFullTreeAsync();
    Task<List<ObjectId>> GetCategoryPathAsync(string categoryId);
}