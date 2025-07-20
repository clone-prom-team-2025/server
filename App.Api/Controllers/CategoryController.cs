using App.Core.DTOs.Categoty;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using App.Core.Models;
using AutoMapper;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;

    public CategoryController(ICategoryService categoryService, IMapper mapper)
    {
        _categoryService = categoryService;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets all categories.
    /// </summary>
    /// <returns>List of categories or empty list.</returns>
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>?>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        if (categories == null || categories.Count == 0)
            return NoContent();

        return Ok(categories);
    }

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>The category or NotFound if missing.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Category?>> GetById(string id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    /// <summary>
    /// Gets a category by localized name.
    /// </summary>
    /// <param name="name">Category name.</param>
    /// <param name="languageCode">Language code (default: "en").</param>
    /// <returns>The category or NotFound.</returns>
    [HttpGet("by-name")]
    public async Task<ActionResult<Category?>> GetByName([FromQuery] string name, [FromQuery] string languageCode = "en")
    {
        var category = await _categoryService.GetByNameAsync(name, languageCode);
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    /// <summary>
    /// Gets direct children categories by parent ID.
    /// </summary>
    /// <param name="parentId">Parent category ID.</param>
    /// <returns>List of child categories or empty list.</returns>
    [HttpGet("children/{parentId}")]
    public async Task<ActionResult<List<Category>?>> GetByParentId(string parentId)
    {
        var children = await _categoryService.GetByParentIdAsync(parentId);
        if (children == null || children.Count == 0)
            return NoContent();

        return Ok(children);
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="categoryDto">Category data.</param>
    /// <returns>Created category.</returns>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CategoryCreateDto categoryDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var category = await _categoryService.CreateAsync(categoryDto);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <param name="categoryDto">Updated category DTO</param>
    /// <returns>NoContent if success; NotFound if category not found.</returns>
    [HttpPut]
    public async Task<ActionResult> Update([FromBody] CategoryDto categoryDto)
    {
        var category = await _categoryService.UpdateAsync(categoryDto);
        if (category == null) return BadRequest(ModelState);

        return Ok(category);
    }

    /// <summary>
    /// Deletes a category by ID.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>NoContent if success; NotFound if category not found.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var deleted = await _categoryService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Searches categories by name with localized highlighting.
    /// </summary>
    /// <param name="name">Search string.</param>
    /// <param name="languageCode">Language code (default: "en").</param>
    /// <returns>List of matching categories.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<List<Category>?>> Search([FromQuery] string name, [FromQuery] string languageCode = "en")
    {
        var results = await _categoryService.SearchAsync(name, languageCode);
        if (results == null || results.Count == 0)
            return NoContent();

        return Ok(results);
    }

    /// <summary>
    /// Gets full ancestry tree of a category.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>CategoryNode representing the ancestry tree or NotFound.</returns>
    [HttpGet("full-tree")]
    public async Task<ActionResult<CategoryNode?>> GetFullTree()
    {
        var tree = await _categoryService.GetFullTreeAsync();
        if (tree == null)
            return NotFound();

        return Ok(tree);
    }

    /// <summary>
    /// Gets full subtree starting from a category.
    /// </summary>
    /// <param name="parentId">Parent category ID.</param>
    /// <returns>CategoryNode subtree or NotFound.</returns>
    [HttpGet("category-tree/{parentId}")]
    public async Task<ActionResult<CategoryNode?>> GetCategoryTree(string parentId)
    {
        var tree = await _categoryService.GetCategoryTreeAsync(parentId);
        if (tree == null)
            return NotFound();

        return Ok(tree);
    }

    /// <summary>
    /// Gets immediate children nodes of a category.
    /// </summary>
    /// <param name="parentId">Parent category ID.</param>
    /// <returns>List of child CategoryNodes.</returns>
    [HttpGet("children-nodes/{parentId}")]
    public async Task<ActionResult<List<CategoryNode>>> GetChildrenNodes(string parentId)
    {
        var children = await _categoryService.GetChildrenAsync(parentId);
        return Ok(children);
    }
}
