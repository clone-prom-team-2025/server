using App.Core.DTOs.Categoty;
using App.Core.Interfaces;
using App.Core.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services.Services;

/// <summary>
/// Service class responsible for handling business logic related to categories.
/// Delegates data access operations to the underlying <see cref="ICategoryRepository" />.
/// </summary>
public class CategoryService(ICategoryRepository categoryRepository, IMapper mapper, ILogger<CategoryService> logger) : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<CategoryService> _logger =  logger;

    /// <summary>
    /// Retrieves all categories asynchronously.
    /// </summary>
    public async Task<List<CategoryDto>?> GetAllAsync()
    {
        using (_logger.BeginScope("GetAllAsync"))
        {
            _logger.LogInformation("GetAllAsync called");
            var result = await _categoryRepository.GetAllAsync();
            _logger.LogInformation("GetAllAsync success");
            return result;
        }
        
    }

    /// <summary>
    /// Retrieves a category by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the category.</param>
    public async Task<CategoryDto?> GetByIdAsync(string id)
    {
        using (_logger.BeginScope("GetByIdAsync")){
            _logger.LogInformation("GetByIdAsync called");
            var result = await _categoryRepository.GetByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Category not found");
                throw new KeyNotFoundException("Category not found");
            }
            _logger.LogInformation("GetByIdAsync success");
            return result;
        }
    }

    /// <summary>
    /// Retrieves a category by its localized name asynchronously.
    /// </summary>
    /// <param name="name">The localized category name to search for.</param>
    public async Task<CategoryDto?> GetByNameAsync(string name)
    {
        using (_logger.BeginScope("GetByNameAsync")){
            _logger.LogInformation("GetByNameAsync called");
            var result = await _categoryRepository.GetByNameAsync(name);
            if (result == null)
            {
                _logger.LogWarning("Category not found");
                throw new KeyNotFoundException("Category not found");
            }
            _logger.LogInformation("GetByNameAsync success");
            return result;
        }
    }

    /// <summary>
    /// Retrieves all direct child categories for a specified parent category asynchronously.
    /// </summary>
    /// <param name="parentId">The parent category's unique identifier.</param>
    public async Task<List<CategoryDto>?> GetByParentIdAsync(string parentId)
    {
        using (_logger.BeginScope("GetByParentIdAsync")) {
            _logger.LogInformation("GetByParentIdAsync called");
            var result = await _categoryRepository.GetByParentIdAsync(parentId);
            if (result == null)
            {
                _logger.LogWarning("Category not found");
                throw new KeyNotFoundException("Category not found");
            }
            return result;
        }
    }

    /// <summary>
    /// Creates a new category asynchronously.
    /// </summary>
    /// <param name="categoryCreateDto">The category object to be created.</param>
    public async Task CreateAsync(CategoryCreateDto categoryCreateDto)
    {
        using (_logger.BeginScope("CreateAsync")) {
            _logger.LogInformation("CreateAsync called");
            var category = _mapper.Map<Category>(categoryCreateDto);
            category.Id = ObjectId.GenerateNewId();
            await _categoryRepository.CreateAsync(category);
            _logger.LogInformation("CreateAsync success");
        }
    }

    /// <summary>
    /// Creates multiple categories asynchronously.
    /// </summary>
    /// <param name="categoryCreateDtoList">A list of categories to be created.</param>
    public async Task CreateManyAsync(List<CategoryCreateDto> categoryCreateDtoList)
    {
        using (_logger.BeginScope("CreateManyAsync")) {
            _logger.LogInformation("CreateManyAsync called");
            var categories = _mapper.Map<List<Category>>(categoryCreateDtoList);
            foreach (var category in categories) category.Id = ObjectId.GenerateNewId();
            await _categoryRepository.CreateManyAsync(categories);
            _logger.LogInformation("CreateManyAsync success");
        }
    }

    /// <summary>
    /// Updates an existing category asynchronously.
    /// </summary>
    /// <param name="categoryUpdateDto">The category object containing updated data.</param>
    public async Task UpdateAsync(CategoryDto categoryUpdateDto)
    {
        using (_logger.BeginScope("UpdateAsync")) {
            _logger.LogInformation("UpdateAsync called");
            var category = _mapper.Map<Category>(categoryUpdateDto);
            var result = await _categoryRepository.UpdateAsync(category);
            if (!result) throw new InvalidOperationException("Could not update category");
            _logger.LogInformation("UpdateAsync success");
        }
    }

    /// <summary>
    /// Deletes a category by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the category to delete.</param>
    public async Task DeleteAsync(string id)
    {
        using (_logger.BeginScope("DeleteAsync")) {
            _logger.LogInformation("DeleteAsync called");
            var result = await _categoryRepository.DeleteAsync(id);
            if (!result) throw new InvalidOperationException("Could not delete category");
            _logger.LogInformation("DeleteAsync success");
        }
    }

    /// <summary>
    /// Performs a localized search for categories by name asynchronously.
    /// </summary>
    /// <param name="name">The search string to look for.</param>
    public async Task<List<CategoryDto>?> SearchAsync(string name)
    {
        using (_logger.BeginScope("SearchAsync")){
            _logger.LogInformation("SearchAsync called");
            var result = await _categoryRepository.SearchAsync(name);
            _logger.LogInformation("SearchAsync success");
            return result;
        }
    }

    /// <summary>
    /// Builds and retrieves the full tree of all categories asynchronously.
    /// </summary>
    public async Task<List<CategoryNode>?> GetFullTreeAsync()
    {
        using  (_logger.BeginScope("GetFullTreeAsync")){
            _logger.LogInformation("GetFullTreeAsync called");
            var result = await _categoryRepository.GetFullTreeAsync();
            _logger.LogInformation("GetFullTreeAsync success");
            return result;
        }
    }

    /// <summary>
    /// Builds and retrieves the subtree starting from the specified category asynchronously.
    /// </summary>
    /// <param name="parentId">The root category's unique identifier.</param>
    public async Task<CategoryNode?> GetCategoryTreeAsync(string parentId)
    {
        using (_logger.BeginScope("GetCategoryTreeAsync")) {
            _logger.LogInformation("GetCategoryTreeAsync called");
            var result = await _categoryRepository.GetCategoryTreeAsync(parentId);
            _logger.LogInformation("GetCategoryTreeAsync success");
            return result;
        }
    }

    /// <summary>
    /// Retrieves all immediate child nodes of the specified category asynchronously.
    /// </summary>
    /// <param name="parentId">The parent category's unique identifier.</param>
    public async Task<List<CategoryNode>?> GetChildrenAsync(string parentId)
    {
        using (_logger.BeginScope("GetChildrenAsync"))
        {
            _logger.LogInformation("GetChildrenAsync called");
            var result = await _categoryRepository.GetChildrenAsync(parentId);
            _logger.LogInformation("GetChildrenAsync success");
            return result;
        }
    }
}