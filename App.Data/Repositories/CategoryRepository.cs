using App.Core.Interfaces;
using App.Core.Models;
using MongoDB.Driver;

namespace App.Data.Repositories;

/// <summary>
/// Repository implementation for managing Category documents in MongoDB.
/// Provides CRUD operations, tree navigation, and localized search functionality.
/// </summary>
public class CategoryRepository(MongoDbContext mongoDbContext) : ICategoryRepository
{
    private readonly IMongoCollection<Category> _categories = mongoDbContext.Categories;

    /// <summary>
    /// Gets all categories from the database.
    /// </summary>
    public async Task<List<Category>?> GetAllAsync()
    {
        return await _categories.Find(FilterDefinition<Category>.Empty).ToListAsync();
    }

    /// <summary>
    /// Retrieves a category by its unique identifier.
    /// </summary>
    /// <param name="id">The category ID.</param>
    public async Task<Category?> GetByIdAsync(string id)
    {
        return await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves a category by its localized name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="languageCode">The language code (default is "en").</param>
    public async Task<Category?> GetByNameAsync(string name, string languageCode = "en")
    {
        return await _categories.Find(c => c.Name.ContainsKey(languageCode) && c.Name[languageCode] == name).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves all direct child categories of a given parent category.
    /// </summary>
    /// <param name="parentId">The parent category ID.</param>
    public async Task<List<Category>?> GetByParentIdAsync(string parentId)
    {
        return await _categories.Find(c => c.ParentId != null && c.ParentId == parentId).ToListAsync();
    }

    /// <summary>
    /// Inserts a new category into the database.
    /// </summary>
    /// <param name="category">The category to create.</param>
    public async Task CreateAsync(Category category)
    {
        await _categories.InsertOneAsync(category);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="category">The category with updated values.</param>
    /// <returns>True if a document was matched and modified; otherwise false.</returns>
    public async Task<bool> UpdateAsync(Category category)
    {
        var result = await _categories.ReplaceOneAsync(c => c.Id == category.Id, category);
        return result.IsAcknowledged && result.MatchedCount > 0;
    }

    /// <summary>
    /// Deletes a category by ID.
    /// </summary>
    /// <param name="id">The category ID to delete.</param>
    /// <returns>True if a document was deleted; otherwise false.</returns>
    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _categories.DeleteOneAsync(c => c.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    /// <summary>
    /// Performs a localized fuzzy search by name, and highlights matched substrings.
    /// </summary>
    /// <param name="name">The name fragment to search.</param>
    /// <param name="languageCode">The language code (default is "en").</param>
    /// <returns>List of categories with names where matches are highlighted using square brackets.</returns>
    public async Task<List<Category>?> SearchAsync(string name, string languageCode = "en")
    {
        var filter = Builders<Category>.Filter.Regex(
            $"Name.{languageCode}",
            new MongoDB.Bson.BsonRegularExpression(name, "i")
        );

        var categories = await _categories.Find(filter).ToListAsync();

        if (categories is null || categories.Count == 0)
            return new List<Category>();

        var results = categories
            .Select(cat =>
            {
                if (!cat.Name.TryGetValue(languageCode, out var localizedName))
                    return null;

                var index = localizedName.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    return new
                    {
                        Category = cat,
                        Rank = int.MaxValue,
                        Highlighted = localizedName
                    };

                var highlighted = localizedName.Substring(0, index) +
                                  "[" + localizedName.Substring(index, name.Length) + "]" +
                                  localizedName.Substring(index + name.Length);

                return new
                {
                    Category = cat,
                    Rank = index,
                    Highlighted = highlighted
                };
            })
            .Where(x => x != null)
            .OrderBy(x => x!.Rank)
            .Select(x =>
            {
                x!.Category.Name[languageCode] = x.Highlighted;
                return x.Category;
            })
            .ToList();

        return results;
    }

    /// <summary>
    /// Builds the full parent tree (ancestry) from a given category ID upward.
    /// Each parent wraps the previous node as a child.
    /// </summary>
    /// <param name="id">The starting category ID.</param>
    /// <returns>The root ancestor with nested children leading to the original category.</returns>
    public async Task<CategoryNode?> GetParentsTreeAsync(string id)
    {
        CategoryNode? childNode = null;
        var current = await _categories.Find(x => x.Id == id).FirstOrDefaultAsync();

        while (current != null)
        {
            var node = new CategoryNode
            {
                Id = current.Id,
                Name = current.Name,
                Children = new()
            };

            if (childNode != null)
            {
                node.Children.Add(childNode);
            }

            childNode = node;

            if (string.IsNullOrEmpty(current.ParentId))
                break;

            current = await _categories.Find(x => x.Id == current.ParentId).FirstOrDefaultAsync();
        }

        return childNode;
    }

    /// <summary>
    /// Builds a full category subtree starting from the specified parent ID.
    /// </summary>
    /// <param name="parentId">The root category ID to start from.</param>
    /// <returns>The root node with all nested children.</returns>
    public async Task<CategoryNode?> GetCategoryTreeAsync(string parentId)
    {
        var parentCategory = await _categories.Find(c => c.Id == parentId).FirstOrDefaultAsync();
        if (parentCategory == null) return null;

        var rootNode = new CategoryNode
        {
            Id = parentCategory.Id,
            Name = parentCategory.Name
        };

        rootNode.Children = await GetChildrenAsync(parentCategory.Id);
        return rootNode;
    }

    /// <summary>
    /// Recursively retrieves all children of a given category as a tree structure.
    /// </summary>
    /// <param name="parentId">The parent category ID.</param>
    /// <returns>List of child nodes with their nested descendants.</returns>
    public async Task<List<CategoryNode>> GetChildrenAsync(string parentId)
    {
        var children = await _categories.Find(c => c.ParentId == parentId).ToListAsync();
        var result = new List<CategoryNode>();

        foreach (var child in children)
        {
            var childNode = new CategoryNode
            {
                Id = child.Id,
                Name = child.Name,
                Children = await GetChildrenAsync(child.Id)
            };

            result.Add(childNode);
        }

        return result;
    }
}
