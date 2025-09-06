using App.Core.DTOs.Categoty;
using App.Core.Interfaces;
using App.Core.Models;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

/// <summary>
///     Repository implementation for managing Category documents in MongoDB.
///     Provides CRUD operations, tree navigation, and localized search functionality.
/// </summary>
public class CategoryRepository(MongoDbContext mongoDbContext, IMapper mapper) : ICategoryRepository
{
    private readonly IMongoCollection<Category> _categories = mongoDbContext.Categories;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    ///     Gets all categories from the database.
    /// </summary>
    public async Task<List<CategoryDto>?> GetAllAsync()
    {
        var categories = await _categories.Find(FilterDefinition<Category>.Empty).ToListAsync();

        return _mapper.Map<List<CategoryDto>?>(categories);
    }

    /// <summary>
    ///     Retrieves a category by its unique identifier.
    /// </summary>
    /// <param name="id">The category ID.</param>
    public async Task<CategoryDto?> GetByIdAsync(string id)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.Id, ObjectId.Parse(id));
        var category = await _categories.Find(filter).FirstOrDefaultAsync();
        return _mapper.Map<CategoryDto?>(category);
    }

    /// <summary>
    ///     Retrieves a category by its localized name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="languageCode">The language code (default is "en").</param>
    public async Task<CategoryDto?> GetByNameAsync(string name)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.Name, name);
        var category = await _categories.Find(filter).FirstOrDefaultAsync();
        return _mapper.Map<CategoryDto?>(category);
    }

    /// <summary>
    ///     Retrieves all direct child categories of a given parent category.
    /// </summary>
    /// <param name="parentId">The parent category ID.</param>
    public async Task<List<CategoryDto>?> GetByParentIdAsync(string parentId)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.ParentId, ObjectId.Parse(parentId));
        var categories = await _categories.Find(filter).ToListAsync();
        return _mapper.Map<List<CategoryDto>?>(categories);
    }

    /// <summary>
    ///     Inserts a new category into the database.
    /// </summary>
    /// <param name="category">The category to create.</param>
    public async Task CreateAsync(Category category)
    {
        await _categories.InsertOneAsync(category);
    }

    public async Task CreateManyAsync(List<Category> categoryList)
    {
        await _categories.InsertManyAsync(categoryList);
    }

    /// <summary>
    ///     Updates an existing category.
    /// </summary>
    /// <param name="category">The category with updated values.</param>
    /// <returns>True if a document was matched; otherwise false.</returns>
    public async Task<bool> UpdateAsync(Category category)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.Id, category.Id);
        var result = await _categories.ReplaceOneAsync(filter, category);
        return result.IsAcknowledged;
    }

    /// <summary>
    ///     Deletes a category by ID.
    /// </summary>
    /// <param name="id">The category ID to delete.</param>
    /// <returns>True if a document was deleted; otherwise false.</returns>
    public async Task<bool> DeleteAsync(string id)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.Id, ObjectId.Parse(id));
        var result = await _categories.DeleteOneAsync(filter);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    /// <summary>
    ///     Performs a localized fuzzy search by name, and highlights matched substrings.
    /// </summary>
    /// <param name="name">The name fragment to search.</param>
    /// <param name="languageCode">The language code (default is "en").</param>
    /// <returns>List of categories with names where matches are highlighted using square brackets.</returns>
    public async Task<List<CategoryDto>?> SearchAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new List<CategoryDto>();

        var filter = Builders<Category>.Filter.Regex(
            "Name",
            new BsonRegularExpression(name, "i")
        );

        var categories = await _categories.Find(filter).ToListAsync();

        if (categories == null || categories.Count == 0)
            return new List<CategoryDto>();

        var results = categories
            .Select(cat =>
            {
                var index = cat.Name?.IndexOf(name, StringComparison.OrdinalIgnoreCase) ?? -1;

                string highlighted;
                int rank;

                if (index < 0)
                {
                    highlighted = cat.Name ?? string.Empty;
                    rank = int.MaxValue;
                }
                else
                {
                    highlighted = cat.Name.Substring(0, index) +
                                  "[" + cat.Name.Substring(index, name.Length) + "]" +
                                  cat.Name.Substring(index + name.Length);
                    rank = index;
                }

                return new
                {
                    Category = cat,
                    Rank = rank,
                    Highlighted = highlighted
                };
            })
            .OrderBy(x => x.Rank)
            .Select(x =>
            {
                x.Category.Name = x.Highlighted;
                return x.Category;
            })
            .ToList();

        return _mapper.Map<List<CategoryDto>?>(results);
    }

    /// <summary>
    ///     Builds the full parent tree (ancestry) from a given category ID upward.
    ///     Each parent wraps the previous node as a child.
    /// </summary>
    /// <param name="id">The starting category ID.</param>
    /// <returns>The root ancestor with nested children leading to the original category.</returns>
    public async Task<CategoryNode?> GetParentsTreeAsync(string id)
    {
        CategoryNode? childNode = null;
        var current = await _categories.Find(x => x.Id.ToString().Equals(id)).FirstOrDefaultAsync();

        while (current != null)
        {
            var node = new CategoryNode
            {
                Id = current.Id.ToString(),
                Name = current.Name,
                Children = new List<CategoryNode>()
            };

            if (childNode != null) node.Children.Add(childNode);

            childNode = node;

            if (current.ParentId == null)
                break;

            var filter = Builders<Category>.Filter.Eq(c => c.Id, current.ParentId);
            current = await _categories.Find(filter).FirstOrDefaultAsync();
        }

        return childNode;
    }

    /// <summary>
    ///     Builds a full category subtree starting from the specified parent ID.
    /// </summary>
    /// <param name="parentId">The root category ID to start from.</param>
    /// <returns>The root node with all nested children.</returns>
    public async Task<CategoryNode?> GetCategoryTreeAsync(string parentId)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.Id, ObjectId.Parse(parentId));
        var parentCategory = await _categories.Find(filter).FirstOrDefaultAsync();
        if (parentCategory == null) return null;

        var rootNode = new CategoryNode
        {
            Id = parentCategory.Id.ToString(),
            Name = parentCategory.Name
        };

        rootNode.Children = await GetChildrenAsync(parentCategory.Id.ToString());
        return rootNode;
    }

    /// <summary>
    ///     Recursively retrieves all children of a given category as a tree structure.
    /// </summary>
    /// <param name="parentId">The parent category ID.</param>
    /// <returns>List of child nodes with their nested descendants.</returns>
    public async Task<List<CategoryNode>> GetChildrenAsync(string parentId)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.ParentId, ObjectId.Parse(parentId));
        var children = await _categories.Find(filter).ToListAsync();
        var result = new List<CategoryNode>();

        foreach (var child in children)
        {
            var childNode = new CategoryNode
            {
                Id = child.Id.ToString(),
                Name = child.Name,
                Children = await GetChildrenAsync(child.Id.ToString())
            };

            result.Add(childNode);
        }

        return result;
    }

    /// <summary>
    ///     Retrieves the full category tree, including all root categories and their nested descendants.
    /// </summary>
    /// <returns>List of root nodes with all nested child categories.</returns>
    public async Task<List<CategoryNode>> GetFullTreeAsync()
    {
        var filter = Builders<Category>.Filter.Eq(c => c.ParentId, null);
        var rootCategories = await _categories.Find(filter).ToListAsync();
        var result = new List<CategoryNode>();

        foreach (var root in rootCategories)
        {
            var rootNode = new CategoryNode
            {
                Id = root.Id.ToString(),
                Name = root.Name,
                Children = await GetChildrenAsync(root.Id.ToString())
            };

            result.Add(rootNode);
        }

        return result;
    }

    public async Task<List<ObjectId>?> GetCategoryPathAsync(string categoryId)
    {
        if (!ObjectId.TryParse(categoryId, out var currentId))
            throw new ArgumentException("Invalid category ID format", nameof(categoryId));

        var path = new List<ObjectId>();

        while (currentId != ObjectId.Empty)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Id, currentId);
            var category = await _categories.Find(filter).FirstOrDefaultAsync();

            if (category == null)
                break;

            path.Add(category.Id);

            if (category.ParentId.HasValue)
                currentId = category.ParentId.Value;
            else
                break;
        }

        return path;
    }
}