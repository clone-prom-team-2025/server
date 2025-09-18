using App.Core.Interfaces;
using App.Core.Models.Product;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class ProductMediaRepository(MongoDbContext mongoDbContext) : IProductMediaRepository
{
    private readonly IMongoCollection<ProductMedia> _productMedia = mongoDbContext.ProductMedia;

    public async Task<List<ProductMedia>?> GetAll()
    {
        return await _productMedia.Find(FilterDefinition<ProductMedia>.Empty).ToListAsync();
    }

    public async Task SaveAsync(ProductMedia media)
    {
        await _productMedia.InsertOneAsync(media);
    }

    public async Task<bool> UpdateAsync(ProductMedia media)
    {
        var filter = Builders<ProductMedia>.Filter.Eq(m => m.Id, media.Id);
        var result = await _productMedia.ReplaceOneAsync(filter, media);
        return result.IsAcknowledged;
    }

    public async Task<bool> RemoveByProdutIdAsync(string productId)
    {
        var filter = Builders<ProductMedia>.Filter.Eq(m => m.ProductId, ObjectId.Parse(productId));
        var result = await _productMedia.DeleteManyAsync(filter);
        return result.IsAcknowledged;
    }

    public async Task<bool> RemoveAsync(string id)
    {
        var filter = Builders<ProductMedia>.Filter.Eq(m => m.Id, ObjectId.Parse(id));
        var result = await _productMedia.DeleteOneAsync(filter);
        return result.IsAcknowledged;
    }

    public async Task<ProductMedia?> GetByIdAsync(string id)
    {
        var filter = Builders<ProductMedia>.Filter.Eq(m => m.Id, ObjectId.Parse(id));
        return await _productMedia.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<ProductMedia>?> GetByProductIdAsync(string productId)
    {
        var filter = Builders<ProductMedia>.Filter.Eq(m => m.ProductId, ObjectId.Parse(productId));
        return await _productMedia.Find(filter).ToListAsync();
    }
    //
    // public async Task<ProductMedia?> GetFilenameAsync(string fileName)
    // {
    //     var filter = Builders<ProductMedia>.Filter.Eq(m => m.UrlFileName, fileName);
    //     return await _productMedia.Find(filter).FirstOrDefaultAsync();
    // }
}