using App.Core.Interfaces;
using App.Core.Models.Product;
using App.Core.Models.Product.Review;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

/// <summary>
/// MongoDB-based repository for managing product data,
/// including filtering, searching, and CRUD operations.
/// </summary>
public class ProductRepository(MongoDbContext mongoDbContext) : IProductRepository
{
    private readonly IMongoCollection<Product> _products = mongoDbContext.Products;

    /// <summary>
    /// Retrieves all products with optional filtering.
    /// </summary>
    /// <param name="filter">Additional filter parameters (currently unused).</param>
    /// <returns>A list of all products, or null if none found.</returns>
    public async Task<List<Product>?> GetAllAsync(ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;

        var finalFilter = builder.Eq(p => p.ProductType, filter.ProductType);

        if (filter.Include != null && filter.Include.Count > 0)
        {
            foreach (var kv in filter.Include)
            {
                var includeFilter = builder.Eq(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= includeFilter;
            }
        }

        if (filter.Exclude != null && filter.Exclude.Count > 0)
        {
            foreach (var kv in filter.Exclude)
            {
                var excludeFilter = builder.Ne(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= excludeFilter;
            }
        }

        var skip = (filter.Page - 1) * filter.PageSize;
        var limit = filter.PageSize;

        return await _products
            .Find(finalFilter)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a product by its unique ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product if found, otherwise null.</returns>
    public async Task<Product?> GetByIdAsync(string id)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, ObjectId.Parse(id));
        return await _products.Find(filter).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves products by localized name (exact match).
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="filter">Additional filter parameters (currently unused).</param>
    /// <returns>List of products with the specified name, or null if none found.</returns>
    public async Task<List<Product>?> GetByNameAsync(string name, ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;

        var languageCode = filter.Language ?? "en";
        var nameFilter = builder.Eq($"Name.{languageCode}", name);

        var finalFilter = nameFilter;

        if (!string.IsNullOrEmpty(filter.ProductType))
        {
            finalFilter &= builder.Eq(p => p.ProductType, filter.ProductType);
        }

        if (filter.Include != null && filter.Include.Count > 0)
        {
            foreach (var kv in filter.Include)
            {
                var includeFilter = builder.Eq(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= includeFilter;
            }
        }

        if (filter.Exclude != null && filter.Exclude.Count > 0)
        {
            foreach (var kv in filter.Exclude)
            {
                var excludeFilter = builder.Ne(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= excludeFilter;
            }
        }

        var skip = (filter.Page - 1) * filter.PageSize;
        var limit = filter.PageSize;

        return await _products
            .Find(finalFilter)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves products based on their top-level category.
    /// </summary>
    /// <param name="categoryId">The first category in the category path.</param>
    /// <param name="filter">Additional filter parameters (currently unused).</param>
    /// <returns>List of products in the specified category, or null if none found.</returns>
    public async Task<List<Product>?> GetByCategoryAsync(string categoryId, ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;

        var finalFilter = builder.Eq("CategoryPath.0", categoryId);

        if (filter.Include != null && filter.Include.Count > 0)
        {
            foreach (var kv in filter.Include)
            {
                var includeFilter = builder.Eq(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= includeFilter;
            }
        }

        if (filter.Exclude != null && filter.Exclude.Count > 0)
        {
            foreach (var kv in filter.Exclude)
            {
                var excludeFilter = builder.Ne(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= excludeFilter;
            }
        }

        var skip = (filter.Page - 1) * filter.PageSize;
        var limit = filter.PageSize;

        return await _products
            .Find(finalFilter)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all products associated with a specific seller.
    /// </summary>
    /// <param name="sellerId">The seller's ID.</param>
    /// <param name="filter">Additional filter parameters (currently unused).</param>
    /// <returns>List of products by the seller, or null if none found.</returns>
    public async Task<List<Product>?> GetBySellerIdAsync(string sellerId, ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;

        var finalFilter = builder.Eq(p => p.SellerId, ObjectId.Parse(sellerId));

        if (filter.Include != null && filter.Include.Count > 0)
        {
            foreach (var kv in filter.Include)
            {
                var includeFilter = builder.Eq(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= includeFilter;
            }
        }

        if (filter.Exclude != null && filter.Exclude.Count > 0)
        {
            foreach (var kv in filter.Exclude)
            {
                var excludeFilter = builder.Ne(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= excludeFilter;
            }
        }

        var skip = (filter.Page - 1) * filter.PageSize;
        var limit = filter.PageSize;

        return await _products
            .Find(finalFilter)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a product by a specific variation model ID.
    /// Returns a copy of the product that contains only the matched variation.
    /// </summary>
    /// <param name="modelId">The variation's model ID.</param>
    /// <param name="filter">Additional filter parameters (currently unused).</param>
    /// <returns>The product with a single variation, or null if not found.</returns>
    public async Task<Product?> GetByModelIdAsync(string modelId, ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;

        var finalFilter = builder.ElemMatch(
            x => x.Variations,
            v => v.ModelId == modelId
        );

        if (filter.Include != null && filter.Include.Count > 0)
        {
            foreach (var kv in filter.Include)
            {
                var includeFilter = builder.Eq(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= includeFilter;
            }
        }

        if (filter.Exclude != null && filter.Exclude.Count > 0)
        {
            foreach (var kv in filter.Exclude)
            {
                var excludeFilter = builder.Ne(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= excludeFilter;
            }
        }

        var product = await _products.Find(finalFilter).FirstOrDefaultAsync();
        var variation = product?.Variations.FirstOrDefault(v => v.ModelId == modelId);

        if (product is null || variation is null)
            return null;

        var newProduct = new Product(product);
        newProduct.Variations.Clear();
        newProduct.Variations.Add(variation);

        return newProduct;
    }


    /// <summary>
    /// Retrieves a list of products by multiple variation model IDs.
    /// Each returned product includes only the matched variation.
    /// </summary>
    /// <param name="modelIds">List of model IDs to search for.</param>
    /// <param name="filter">Additional filter parameters (currently unused).</param>
    /// <returns>List of products with only one matching variation each, or null if none found.</returns>
    public async Task<List<Product>?> GetByModelIdsAsync(List<string> modelIds, ProductFilterRequest filter)
    {
        if (modelIds == null || modelIds.Count == 0)
            return null;

        var builder = Builders<Product>.Filter;

        var finalFilter = builder.ElemMatch(
            x => x.Variations,
            v => modelIds.Contains(v.ModelId)
        );

        if (filter.Include != null && filter.Include.Count > 0)
        {
            foreach (var kv in filter.Include)
            {
                var includeFilter = builder.Eq(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= includeFilter;
            }
        }

        if (filter.Exclude != null && filter.Exclude.Count > 0)
        {
            foreach (var kv in filter.Exclude)
            {
                var excludeFilter = builder.Ne(kv.Key, BsonValue.Create(kv.Value));
                finalFilter &= excludeFilter;
            }
        }

        var matchedProducts = await _products.Find(finalFilter).ToListAsync();

        if (matchedProducts.Count == 0)
            return null;

        var result = new List<Product>();

        foreach (var product in matchedProducts)
        {
            var matchingVariations = product.Variations
                .Where(v => modelIds.Contains(v.ModelId))
                .ToList();

            foreach (var variation in matchingVariations)
            {
                var newProduct = new Product(product);
                newProduct.Variations.Clear();
                newProduct.Variations.Add(variation);
                result.Add(newProduct);
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a new product in the database.
    /// </summary>
    /// <param name="product">The product to insert.</param>
    public async Task CreateAsync(Product product)
    {
        await _products.InsertOneAsync(product);
    }
    
    /// <summary>
    /// Replaces an existing product document with a new version.
    /// </summary>
    /// <param name="product">The updated product.</param>
    /// <returns>True if the update succeeded, otherwise false.</returns>
    public async Task<bool> UpdateAsync(Product product)
    {
        var result = await _products.ReplaceOneAsync(p => p.Id.Equals(product.Id), product);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="id">The product ID to delete.</param>
    /// <returns>True if the product was deleted, otherwise false.</returns>
    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _products.DeleteOneAsync(p => p.Id.Equals(id));
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    /// <summary>
    /// Searches for products by localized name or variation model name.
    /// Supports partial case-insensitive matching with basic highlighting.
    /// </summary>
    /// <param name="name">The name or substring to search for.</param>
    /// <param name="languageCode">The localization language code (e.g., "en", "ua").</param>
    /// <returns>List of search results with highlighted matches and rankings.</returns>
    public async Task<List<ProductSearchResult>?> SearchByNameAsync(string name, string languageCode = "en")
    {
        var nameRegex = new BsonRegularExpression(name, "i");

        var nameFilter = Builders<Product>.Filter.Regex($"Name.{languageCode}", nameRegex);
        var modelFilter = Builders<Product>.Filter.ElemMatch(p => p.Variations, v => v.ModelName.ToLower().Contains(name.ToLower()));
        var filter = Builders<Product>.Filter.Or(nameFilter, modelFilter);

        var products = await _products.Find(filter).ToListAsync();

        var results = new List<ProductSearchResult>();

        foreach (var product in products)
        {
            if (product.Name.TryGetValue(languageCode, out var localizedName))
            {
                var index = localizedName.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    var highlighted = localizedName[..index] +
                                      "[" + localizedName.Substring(index, name.Length) + "]" +
                                      localizedName[(index + name.Length)..];

                    results.Add(new ProductSearchResult
                    {
                        ProductId = product.Id,
                        Highlighted = highlighted,
                        Rank = index
                    });

                    continue;
                }
            }

            foreach (var variation in product.Variations)
            {
                if (!string.IsNullOrEmpty(variation.ModelName))
                {
                    var index = variation.ModelName.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        var highlighted = variation.ModelName[..index] +
                                          "[" + variation.ModelName.Substring(index, name.Length) + "]" +
                                          variation.ModelName[(index + name.Length)..];

                        results.Add(new ProductSearchResult
                        {
                            ProductId = product.Id,
                            Highlighted = highlighted,
                            Rank = index
                        });

                        break;
                    }
                }
            }
        }

        return results
            .OrderBy(r => r.Rank)
            .ToList();
    }
}
