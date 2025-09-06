using App.Core.Constants;
using App.Core.DTOs.Categoty;
using App.Core.Interfaces;
using App.Core.Models;
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

    public CategoryController(ICategoryService categoryService, IMapper mapper)
    {
        _categoryService = categoryService;
        _mapper = mapper;
    }

    /// <summary>
    ///     Gets all categories.
    /// </summary>
    /// <returns>List of categories or empty list.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            if (categories == null || categories.Count == 0)
                return NoContent();

            return Ok(categories);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
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
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
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
        try
        {
            var children = await _categoryService.GetByParentIdAsync(parentId);
            if (children == null || children.Count == 0)
                return NoContent();

            return Ok(children);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
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
        try
        {
            await _categoryService.CreateAsync(categoryDto);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
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
        try
        {
            await _categoryService.UpdateAsync(categoryDto);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
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
        try
        {
            await _categoryService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
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
        try
        {
            var results = await _categoryService.SearchAsync(name);
            if (results == null || results.Count == 0)
                return NoContent();

            return Ok(results);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
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
        try
        {
            var tree = await _categoryService.GetFullTreeAsync();
            if (tree == null)
                return NotFound();

            return Ok(tree);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
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
        try
        {
            var tree = await _categoryService.GetCategoryTreeAsync(parentId);
            if (tree == null)
                return NotFound();

            return Ok(tree);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
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
        try
        {
            var children = await _categoryService.GetChildrenAsync(parentId);
            return Ok(children);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("many")]
    public async Task<IActionResult> CreateMany([FromBody] List<CategoryCreateDto> categoryList)
    {
        try
        {
            await _categoryService.CreateManyAsync(categoryList);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}