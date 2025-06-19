using App.Core.Models;

namespace App.Core.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>?> GetAllAsync();
    Task<Category?> GetByIdAsync(string id);
    Task<Category?> GetByNameAsync(string name, string languageCode);
    Task<List<Category>?> GetByParentIdAsync(string patrentId);
    Task CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(string id);
    Task<List<Category>?> SearchAsync(string name, string languageCode);
    Task<CategoryNode?> GetParentsTreeAsync(string id);
    Task<CategoryNode?> GetCategoryTreeAsync(string parentId);
    Task<List<CategoryNode>> GetChildrenAsync(string parentId);
}