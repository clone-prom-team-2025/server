using App.Core.Interfaces;
using App.Core.Models.Product;
using App.Core.Models.Product.Review;
using MongoDB.Bson;
using MongoDB.Driver;
using App.Core.Enums;
using SortDirection = App.Core.Enums.SortDirection;

namespace App.Data.Repositories;

/// <summary>
/// MongoDB-based repository for managing product data,
/// including filtering, searching, and CRUD operations.
/// </summary>
public class ProductRepository(MongoDbContext mongoDbContext) : IProductRepository
{
    private readonly IMongoCollection<Product> _products = mongoDbContext.Products;

    private List<FilterDefinition<Product>> FormFilter(ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>();

        if (filter.CategoryId.HasValue)
        {
            filters.Add(builder.AnyEq(p => p.CategoryPath, filter.CategoryId.Value));
        }

        foreach (var kv in filter.Include)
        {
            var includeFilter = builder.ElemMatch(p => p.Variations,
                variation => variation.Features.Any(f =>
                    f.Features.ContainsKey(kv.Key) && f.Features[kv.Key].Value == kv.Value
                )
            );

            filters.Add(includeFilter);
        }

        foreach (var kv in filter.Exclude)
        {
            var excludeFilter = builder.Not(
                builder.ElemMatch(p => p.Variations,
                    variation => variation.Features.Any(f =>
                        f.Features.ContainsKey(kv.Key) && f.Features[kv.Key].Value == kv.Value
                    )
                )
            );

            filters.Add(excludeFilter);
        }

        return filters;
    }
    
    private SortDefinition<Product>? BuildSort(SortDirection sortDirection)
    {
        var sortBuilder = Builders<Product>.Sort;

        return sortDirection switch
        {
            SortDirection.PriceAsc => sortBuilder.Ascending(p => p.MinPrice),
            SortDirection.PriceDesc => sortBuilder.Descending(p => p.MaxPrice),
            _ => null
        };
    }
    
    private void UpdatePriceRange(Product product)
    {
        if (product.Variations.Any())
        {
            product.MinPrice = product.Variations.Min(v => v.Price);
            product.MaxPrice = product.Variations.Max(v => v.Price);
        }
    }
    
    public async Task<List<Product>?> GetAllAsync(ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;
        var filters = FormFilter(filter);
        var finalFilter = filters.Any() ? builder.And(filters) : builder.Empty;

        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Max(filter.PageSize, 1);

        var skip = (page - 1) * pageSize;
        var limit = pageSize;

        var sortDefinition = BuildSort(filter.Sort);

        var query = _products.Find(finalFilter);

        if (sortDefinition is not null)
        {
            query = query.Sort(sortDefinition);
        }
        
        

        return await query
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, ObjectId.Parse(id));
        return await _products.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<Product>?> GetByNameAsync(string name, ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;

        var filters = FormFilter(filter);
        filters.Add(builder.Eq($"Name.{filter.Language}", name));

        var finalFilter = filters.Any() ? builder.And(filters) : builder.Empty;

        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Max(filter.PageSize, 1);

        var skip = (page - 1) * pageSize;
        var limit = pageSize;

        var sortDefinition = BuildSort(filter.Sort);

        var query = _products.Find(finalFilter);

        if (sortDefinition is not null)
        {
            query = query.Sort(sortDefinition);
        }

        return await query
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<List<Product>?> GetByCategoryAsync(string categoryId, ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;

        var filters = FormFilter(filter);

        filters.Add(builder.Eq("Category.0", categoryId));

        var finalFilter = filters.Any() ? builder.And(filters) : builder.Empty;

        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Max(filter.PageSize, 1);

        var skip = (page - 1) * pageSize;
        var limit = pageSize;

        var sortDefinition = BuildSort(filter.Sort);

        var query = _products.Find(finalFilter);

        if (sortDefinition is not null)
        {
            query = query.Sort(sortDefinition);
        }

        return await query
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<List<Product>?> GetBySellerIdAsync(string sellerId, ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;

        var filters = FormFilter(filter);
        filters.Add(builder.Eq(p => p.SellerId, ObjectId.Parse(sellerId)));

        var finalFilter = filters.Any() ? builder.And(filters) : builder.Empty;

        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Max(filter.PageSize, 1);

        var skip = (page - 1) * pageSize;
        var limit = pageSize;

        var sortDefinition = BuildSort(filter.Sort);

        var query = _products.Find(finalFilter);

        if (sortDefinition is not null)
        {
            query = query.Sort(sortDefinition);
        }

        return await query
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<Product?> GetByModelIdAsync(string modelId, ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;

        var filters = FormFilter(filter);
        filters.Add(builder.ElemMatch(
            x => x.Variations,
            v => v.ModelId == modelId
        ));

        var finalFilter = filters.Any() ? builder.And(filters) : builder.Empty;
        
        var sortDefinition = BuildSort(filter.Sort);

        var query = _products.Find(finalFilter);

        if (sortDefinition is not null)
        {
            query = query.Sort(sortDefinition);
        }

        var product = await query.FirstOrDefaultAsync();
        var variation = product?.Variations.FirstOrDefault(v => v.ModelId == modelId);

        if (product is null || variation is null)
            return null;

        var newProduct = new Product(product);
        newProduct.Variations.Clear();
        newProduct.Variations.Add(variation);
        newProduct.MinPrice = variation.Price;
        newProduct.MaxPrice = variation.Price;

        return newProduct;
    }

    public async Task<List<Product>?> GetByModelIdsAsync(List<string> modelIds, ProductFilterRequest filter)
    {
        if (modelIds.Count == 0)
            return null;

        var builder = Builders<Product>.Filter;

        var filters = FormFilter(filter);
        filters.Add(builder.ElemMatch(
            x => x.Variations,
            v => modelIds.Contains(v.ModelId)
        )
        );

        var finalFilter = filters.Any() ? builder.And(filters) : builder.Empty;

        var sortDefinition = BuildSort(filter.Sort);

        var query = _products.Find(finalFilter);

        if (sortDefinition is not null)
        {
            query = query.Sort(sortDefinition);
        }
        
        var matchedProducts = await query.ToListAsync();
        
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
                newProduct.MinPrice = variation.Price;
                newProduct.MaxPrice = variation.Price;
                result.Add(newProduct);
            }
        }

        return result;
    }

    public async Task CreateAsync(Product product)
    {
        UpdatePriceRange(product);
        await _products.InsertOneAsync(product);
    }
    
    public async Task<bool> UpdateAsync(Product product)
    {
        UpdatePriceRange(product);
        var result = await _products.ReplaceOneAsync(p => p.Id.Equals(product.Id), product);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, ObjectId.Parse(id));
        var result = await _products.DeleteOneAsync(filter);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

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
