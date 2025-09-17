using App.Core.DTOs.Categoty;
using App.Core.Interfaces;
using App.Core.Models;
using App.Data.Repositories;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Services;

/// <summary>
///     Service class responsible for handling business logic related to categories.
///     Delegates data access operations to the underlying <see cref="CategoryRepository" />.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="CategoryService" /> class.
/// </remarks>
/// <param name="categoryRepository">The repository instance used for data access.</param>
/// <param name="mapper">The mapper instance used for mapping objects</param>
public class CategoryService(ICategoryRepository categoryRepository, IMapper mapper) : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    ///     Retrieves all categories asynchronously.
    /// </summary>
    /// <returns>A list of all categories or null if none found.</returns>
    public async Task<List<CategoryDto>?> GetAllAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }

    /// <summary>
    ///     Retrieves a category by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the category.</param>
    /// <returns>The category if found; otherwise, null.</returns>
    public async Task<CategoryDto?> GetByIdAsync(string id)
    {
        return await _categoryRepository.GetByIdAsync(id);
    }

    /// <summary>
    ///     Retrieves a category by its localized name asynchronously.
    /// </summary>
    /// <param name="name">The localized category name to search for.</param>
    /// <param name="languageCode">The language code for localization (e.g., "en").</param>
    /// <returns>The matching category if found; otherwise, null.</returns>
    public async Task<CategoryDto?> GetByNameAsync(string name)
    {
        return await _categoryRepository.GetByNameAsync(name);
    }

    /// <summary>
    ///     Retrieves all direct child categories for a specified parent category asynchronously.
    /// </summary>
    /// <param name="parentId">The parent category's unique identifier.</param>
    /// <returns>A list of child categories or null if none found.</returns>
    public async Task<List<CategoryDto>?> GetByParentIdAsync(string parentId)
    {
        return await _categoryRepository.GetByParentIdAsync(parentId);
    }

    /// <summary>
    ///     Creates a new category asynchronously.
    /// </summary>
    /// <param name="categoryCreateDto">The category object to be created.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CreateAsync(CategoryCreateDto categoryCreateDto)
    {
        var category = _mapper.Map<Category>(categoryCreateDto);
        category.Id = ObjectId.GenerateNewId();
        await _categoryRepository.CreateAsync(category);
    }

    public async Task CreateManyAsync(List<CategoryCreateDto> categoryCreateDtoList)
    {
        var categories = _mapper.Map<List<Category>>(categoryCreateDtoList);
        foreach (var category in categories) category.Id = ObjectId.GenerateNewId();
        await _categoryRepository.CreateManyAsync(categories);
    }

    /// <summary>
    ///     Updates an existing category asynchronously.
    /// </summary>
    /// <param name="categoryUpdateDto">The category object containing updated data.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    public async Task UpdateAsync(CategoryDto categoryUpdateDto)
    {
        var category = _mapper.Map<Category>(categoryUpdateDto);
        var result = await _categoryRepository.UpdateAsync(category);
        if (!result) throw new InvalidOperationException("Could not update category");
        var updatedCategory = await _categoryRepository.GetByIdAsync(category.Id.ToString());
    }

    /// <summary>
    ///     Deletes a category by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the category to delete.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
    public async Task DeleteAsync(string id)
    {
        await _categoryRepository.DeleteAsync(id);
    }

    /// <summary>
    ///     Performs a localized search for categories by name asynchronously.
    /// </summary>
    /// <param name="name">The search string to look for.</param>
    /// <param name="languageCode">The language code for localization (e.g., "en").</param>
    /// <returns>A list of matching categories with highlights or an empty list.</returns>
    public async Task<List<CategoryDto>?> SearchAsync(string name)
    {
        return await _categoryRepository.SearchAsync(name);
    }

    /// <summary>
    ///     Builds and retrieves the full tree of a category.
    /// </summary>
    /// <returns>A <see cref="CategoryNode" /> representing the ancestry tree or null if not found.</returns>
    public async Task<List<CategoryNode>?> GetFullTreeAsync()
    {
        return await _categoryRepository.GetFullTreeAsync();
    }

    /// <summary>
    ///     Builds and retrieves the full subtree starting from the specified category asynchronously.
    /// </summary>
    /// <param name="parentId">The root category's unique identifier.</param>
    /// <returns>A <see cref="CategoryNode" /> representing the subtree or null if not found.</returns>
    public async Task<CategoryNode?> GetCategoryTreeAsync(string parentId)
    {
        return await _categoryRepository.GetCategoryTreeAsync(parentId);
    }

    /// <summary>
    ///     Retrieves all immediate child nodes of the specified category asynchronously.
    /// </summary>
    /// <param name="parentId">The parent category's unique identifier.</param>
    /// <returns>A list of <see cref="CategoryNode" /> representing the children.</returns>
    public async Task<List<CategoryNode>?> GetChildrenAsync(string parentId)
    {
        return await _categoryRepository.GetChildrenAsync(parentId);
    }
}