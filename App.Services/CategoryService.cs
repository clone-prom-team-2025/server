using App.Core.Interfaces;
using App.Core.Models;
using App.Data.Repositories;

namespace App.Services;

/// <summary>
/// Service class responsible for handling business logic related to categories.
/// Delegates data access operations to the underlying <see cref="CategoryRepository"/>.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryService"/> class.
    /// </summary>
    /// <param name="categoryRepository">The repository instance used for data access.</param>
    public CategoryService(ICategoryRepository categoryRepository)
    {
        this._categoryRepository = categoryRepository;
    }

    /// <summary>
    /// Retrieves all categories asynchronously.
    /// </summary>
    /// <returns>A list of all categories or null if none found.</returns>
    public async Task<List<Category>?> GetAllAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }

    /// <summary>
    /// Retrieves a category by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the category.</param>
    /// <returns>The category if found; otherwise, null.</returns>
    public async Task<Category?> GetByIdAsync(string id)
    {
        return await _categoryRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Retrieves a category by its localized name asynchronously.
    /// </summary>
    /// <param name="name">The localized category name to search for.</param>
    /// <param name="languageCode">The language code for localization (e.g., "en").</param>
    /// <returns>The matching category if found; otherwise, null.</returns>
    public async Task<Category?> GetByNameAsync(string name, string languageCode)
    {
        return await _categoryRepository.GetByNameAsync(name, languageCode);
    }

    /// <summary>
    /// Retrieves all direct child categories for a specified parent category asynchronously.
    /// </summary>
    /// <param name="parentId">The parent category's unique identifier.</param>
    /// <returns>A list of child categories or null if none found.</returns>
    public async Task<List<Category>?> GetByParentIdAsync(string parentId)
    {
        return await _categoryRepository.GetByParentIdAsync(parentId);
    }

    /// <summary>
    /// Creates a new category asynchronously.
    /// </summary>
    /// <param name="category">The category object to be created.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CreateAsync(Category category)
    {
        await _categoryRepository.CreateAsync(category);
    }

    /// <summary>
    /// Updates an existing category asynchronously.
    /// </summary>
    /// <param name="category">The category object containing updated data.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    public async Task<bool> UpdateAsync(Category category)
    {
        return await _categoryRepository.UpdateAsync(category);
    }

    /// <summary>
    /// Deletes a category by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the category to delete.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(string id)
    {
        return await _categoryRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Performs a localized search for categories by name asynchronously.
    /// </summary>
    /// <param name="name">The search string to look for.</param>
    /// <param name="languageCode">The language code for localization (e.g., "en").</param>
    /// <returns>A list of matching categories with highlights or an empty list.</returns>
    public async Task<List<Category>?> SearchAsync(string name, string languageCode)
    {
        return await _categoryRepository.SearchAsync(name, languageCode);
    }

    /// <summary>
    /// Builds and retrieves the full ancestry tree of a category up to the root asynchronously.
    /// </summary>
    /// <param name="id">The category's unique identifier.</param>
    /// <returns>A <see cref="CategoryNode"/> representing the ancestry tree or null if not found.</returns>
    public async Task<CategoryNode?> GetParentsTreeAsync(string id)
    {
        return await _categoryRepository.GetParentsTreeAsync(id);
    }

    /// <summary>
    /// Builds and retrieves the full subtree starting from the specified category asynchronously.
    /// </summary>
    /// <param name="parentId">The root category's unique identifier.</param>
    /// <returns>A <see cref="CategoryNode"/> representing the subtree or null if not found.</returns>
    public async Task<CategoryNode?> GetCategoryTreeAsync(string parentId)
    {
        return await _categoryRepository.GetCategoryTreeAsync(parentId);
    }

    /// <summary>
    /// Retrieves all immediate child nodes of the specified category asynchronously.
    /// </summary>
    /// <param name="parentId">The parent category's unique identifier.</param>
    /// <returns>A list of <see cref="CategoryNode"/> representing the children.</returns>
    public async Task<List<CategoryNode>> GetChildrenAsync(string parentId)
    {
        return await _categoryRepository.GetChildrenAsync(parentId);
    }
}
