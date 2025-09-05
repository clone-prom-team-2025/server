using System.Text.RegularExpressions;
using App.Core.Interfaces;
using App.Core.Models.Product;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

/// <summary>
///     MongoDB-based repository for managing product data,
///     including filtering, searching, and CRUD operations.
/// </summary>
public class ProductRepository(MongoDbContext mongoDbContext) : IProductRepository
{
    private readonly IMongoCollection<Product> _products = mongoDbContext.Products;

    public async Task<ProductFilterResponse?> GetAllAsync(ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;
        var filters = FormFilter(filter);
        var finalFilter = filters.Any() ? builder.And(filters) : builder.Empty;

        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Max(filter.PageSize, 1);

        var skip = (page - 1) * pageSize;
        var limit = pageSize;

        var products = await _products
            .Find(finalFilter)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

        var totalCount = await _products.CountDocumentsAsync(finalFilter);

        var pages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var priceRange = await _products
            .Aggregate()
            .Match(finalFilter)
            .Group(new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "minPrice", new BsonDocument("$min", "$Price") },
                { "maxPrice", new BsonDocument("$max", "$Price") }
            })
            .FirstOrDefaultAsync();

        var priceFrom = priceRange?["minPrice"].AsDecimal ?? 0;
        var priceTo = priceRange?["maxPrice"].AsDecimal ?? 0;

        return new ProductFilterResponse
        {
            PriceFrom = priceFrom,
            PriceTo = priceTo,
            Pages = pages,
            Products = products,
            Count = products.Count
        };
    }

    public async Task<Product?> GetByIdAsync(ObjectId id)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
        return await _products.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<ProductFilterResponse?> GetByNameAsync(string name, ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;

        var filters = FormFilter(filter);

        var finalFilter = filters.Any() ? builder.And(filters) : builder.Empty;

        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Max(filter.PageSize, 1);

        var skip = (page - 1) * pageSize;
        var limit = pageSize;

        var products = await _products
            .Find(finalFilter)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

        var totalCount = await _products.CountDocumentsAsync(finalFilter);

        var pages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var priceRange = await _products
            .Aggregate()
            .Match(finalFilter)
            .Group(new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "minPrice", new BsonDocument("$min", "$Price") },
                { "maxPrice", new BsonDocument("$max", "$Price") }
            })
            .FirstOrDefaultAsync();

        var priceFrom = priceRange?["minPrice"].AsDecimal ?? 0;
        var priceTo = priceRange?["maxPrice"].AsDecimal ?? 0;

        return new ProductFilterResponse
        {
            PriceFrom = priceFrom,
            PriceTo = priceTo,
            Pages = pages,
            Products = products,
            Count = products.Count
        };
    }

    public async Task<ProductFilterResponse?> GetBySellerIdAsync(ObjectId sellerId, ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;

        var filters = FormFilter(filter);
        filters.Add(builder.Eq(p => p.SellerId, sellerId));

        var finalFilter = filters.Any() ? builder.And(filters) : builder.Empty;

        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Max(filter.PageSize, 1);

        var skip = (page - 1) * pageSize;
        var limit = pageSize;

        var products = await _products
            .Find(finalFilter)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

        var totalCount = await _products.CountDocumentsAsync(finalFilter);

        var pages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var priceRange = await _products
            .Aggregate()
            .Match(finalFilter)
            .Group(new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "minPrice", new BsonDocument("$min", "$Price") },
                { "maxPrice", new BsonDocument("$max", "$Price") }
            })
            .FirstOrDefaultAsync();

        var priceFrom = priceRange?["minPrice"].AsDecimal ?? 0;
        var priceTo = priceRange?["maxPrice"].AsDecimal ?? 0;

        return new ProductFilterResponse
        {
            PriceFrom = priceFrom,
            PriceTo = priceTo,
            Pages = pages,
            Products = products,
            Count = products.Count
        };
    }

    public async Task<Product> CreateAsync(Product product)
    {
        await _products.InsertOneAsync(product);
        return product;
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        var result = await _products.ReplaceOneAsync(p => p.Id.Equals(product.Id), product);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(ObjectId id)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
        var result = await _products.DeleteOneAsync(filter);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public async Task<List<ProductSearchResult>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new List<ProductSearchResult>();

        var normalizedName = Regex.Replace(name.Trim(), @"\s+", "");

        var pattern = string.Join(@"\s*", normalizedName.Select(c => Regex.Escape(c.ToString())));
        var nameRegex = new BsonRegularExpression(pattern, "i");

        var filter = Builders<Product>.Filter.Regex(p => p.Name, nameRegex);
        var products = await _products.Find(filter).ToListAsync();

        var results = new List<ProductSearchResult>(products.Count);

        foreach (var product in products)
        {
            var cleanProductName = Regex.Replace(product.Name, @"\s+", "");
            var index = cleanProductName.IndexOf(normalizedName, StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                var original = product.Name;

                var originalStartIndex = FindOriginalIndexWithoutSpaces(original, index);

                var originalEndIndex = FindOriginalEndIndex(original, originalStartIndex, normalizedName.Length);

                var highlighted = original[..originalStartIndex] +
                                  "[" + original.Substring(originalStartIndex, originalEndIndex - originalStartIndex) +
                                  "]" +
                                  original[originalEndIndex..];

                results.Add(new ProductSearchResult
                {
                    ProductId = product.Id,
                    Highlighted = highlighted,
                    Rank = index
                });
            }
        }

        return results
            .OrderBy(r => r.Rank)
            .ToList();
    }

    private List<FilterDefinition<Product>> FormFilter(ProductFilterRequest filter)
    {
        var builder = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>();

        if (filter.CategoryId.HasValue) filters.Add(builder.AnyEq(p => p.CategoryPath, filter.CategoryId.Value));

        if (filter.PriceMin.HasValue && filter.PriceMin > 0)
            filters.Add(builder.Gte(p => p.Price, filter.PriceMin.Value));

        if (filter.PriceMax.HasValue && filter.PriceMax > 0)
            filters.Add(builder.Lte(p => p.Price, filter.PriceMax.Value));

        foreach (var kv in filter.Include)
        {
            var includeFilter = builder.ElemMatch(p => p.Features,
                feature => feature.Features.ContainsKey(kv.Key) &&
                           feature.Features[kv.Key].Value == kv.Value
            );

            filters.Add(includeFilter);
        }

        foreach (var kv in filter.Exclude)
        {
            var excludeFilter = builder.Not(
                builder.ElemMatch(p => p.Features,
                    feature => feature.Features.ContainsKey(kv.Key) &&
                               feature.Features[kv.Key].Value == kv.Value
                )
            );

            filters.Add(excludeFilter);
        }

        return filters;
    }

    private static int FindOriginalIndexWithoutSpaces(string text, int indexInClean)
    {
        var cleanCount = 0;
        for (var i = 0; i < text.Length; i++)
            if (!char.IsWhiteSpace(text[i]))
            {
                if (cleanCount == indexInClean)
                    return i;
                cleanCount++;
            }

        return text.Length;
    }

    private static int FindOriginalEndIndex(string text, int startIndex, int lengthWithoutSpaces)
    {
        var cleanCount = 0;
        for (var i = startIndex; i < text.Length; i++)
            if (!char.IsWhiteSpace(text[i]))
            {
                cleanCount++;
                if (cleanCount == lengthWithoutSpaces)
                    return i + 1;
            }

        return text.Length;
    }
}