using App.Core.Constants;
using App.Core.DTOs.Categoty;
using App.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, IMapper mapper, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    ///     Gets all categories.
    /// </summary>
    /// <returns>List of categories or empty list.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        using (_logger.BeginScope("GetAll")){
            _logger.LogInformation("GetAll action");
            var categories = await _categoryService.GetAllAsync();
            if (categories == null || categories.Count == 0)
                return NoContent();
            _logger.LogInformation("GetAll success");

            return Ok(categories);
        }
    }

    /// <summary>
    ///     Gets a category by ID.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>The category or NotFound if missing.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        using (_logger.BeginScope("GetById")) {
            _logger.LogInformation("GetById action");
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();
            _logger.LogInformation("GetById success");

            return Ok(category);
        }
    }

    /// <summary>
    ///     Gets direct children categories by parent ID.
    /// </summary>
    /// <param name="parentId">Parent category ID.</param>
    /// <returns>List of child categories or empty list.</returns>
    [HttpGet("children/{parentId}")]
    public async Task<IActionResult> GetByParentId(string parentId)
    {
        using (_logger.BeginScope("GetByParentId")) {
            _logger.LogInformation("GetByParentId action");
            var children = await _categoryService.GetByParentIdAsync(parentId);
            if (children == null || children.Count == 0)
                return NoContent();
            _logger.LogInformation("GetByParentId success");

            return Ok(children);
        }
    }

    /// <summary>
    ///     Creates a new category.
    /// </summary>
    /// <param name="categoryDto">Category data.</param>
    /// <returns>Created category.</returns>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto categoryDto)
    {
        using (_logger.BeginScope("Create")) {
            _logger.LogInformation("Create action");
            await _categoryService.CreateAsync(categoryDto);
            _logger.LogInformation("Create success");
            return NoContent();
        }
    }

    /// <summary>
    ///     Updates an existing category.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <param name="categoryDto">Updated category DTO</param>
    /// <returns>NoContent if success; NotFound if category not found.</returns>
    [HttpPut]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Update([FromBody] CategoryDto categoryDto)
    {
        using (_logger.BeginScope("Update")) {
            _logger.LogInformation("Update action");
            await _categoryService.UpdateAsync(categoryDto);
            _logger.LogInformation("Update success");
            return NoContent();
        }
    }

    /// <summary>
    ///     Deletes a category by ID.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>NoContent if success; NotFound if category not found.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(string id)
    {
        using (_logger.BeginScope("Delete")) {
            _logger.LogInformation("Delete action");
            await _categoryService.DeleteAsync(id);
            _logger.LogInformation("Delete success");
            return NoContent();
        }
    }

    /// <summary>
    ///     Searches categories by name with localized highlighting.
    /// </summary>
    /// <param name="name">Search string.</param>
    /// <param name="languageCode">Language code (default: "en").</param>
    /// <returns>List of matching categories.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search(string name)
    {
        using (_logger.BeginScope("Search")) {
            _logger.LogInformation("Search action");
            var results = await _categoryService.SearchAsync(name);
            if (results == null || results.Count == 0)
                return NoContent();
            _logger.LogInformation("Search success");

            return Ok(results);
        }
    }

    /// <summary>
    ///     Gets full ancestry tree of a category.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>CategoryNode representing the ancestry tree or NotFound.</returns>
    [HttpGet("full-tree")]
    public async Task<IActionResult> GetFullTree()
    {
        using (_logger.BeginScope("GetFullTree")) {
            _logger.LogInformation("GetFullTree action");
            var tree = await _categoryService.GetFullTreeAsync();
            if (tree == null)
                return NotFound();
            _logger.LogInformation("GetFullTree success");

            return Ok(tree);
        }
    }

    /// <summary>
    ///     Gets full subtree starting from a category.
    /// </summary>
    /// <param name="parentId">Parent category ID.</param>
    /// <returns>CategoryNode subtree or NotFound.</returns>
    [HttpGet("category-tree/{parentId}")]
    public async Task<IActionResult> GetCategoryTree(string parentId)
    {
        using (_logger.BeginScope("GetCategoryTree")) {
            _logger.LogInformation("GetCategoryTree action");
            var tree = await _categoryService.GetCategoryTreeAsync(parentId);
            if (tree == null)
                return NotFound();
            _logger.LogInformation("GetCategoryTree success");

            return Ok(tree);
        }
    }

    /// <summary>
    ///     Gets immediate children nodes of a category.
    /// </summary>
    /// <param name="parentId">Parent category ID.</param>
    /// <returns>List of child CategoryNodes.</returns>
    [HttpGet("children-nodes/{parentId}")]
    public async Task<IActionResult> GetChildrenNodes(string parentId)
    {
        using (_logger.BeginScope("GetChildrenNodes")) {
            _logger.LogInformation("GetChildrenNodes action");
            var children = await _categoryService.GetChildrenAsync(parentId);
            _logger.LogInformation("GetChildrenNodes success");
            return Ok(children);
        }
    }

    [HttpPost("many")]
    public async Task<IActionResult> CreateMany([FromBody] List<CategoryCreateDto> categoryList)
    {
        using (_logger.BeginScope("CreateMany")) {
            _logger.LogInformation("CreateMany action");
            await _categoryService.CreateManyAsync(categoryList);
            _logger.LogInformation("CreateMany success");
            return NoContent();
        }
    }
}