using App.Core.Interfaces;
using App.Core.Models.Favorite;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class FavoriteProductRepository(MongoDbContext mongoDbContext) : IFavoriteProductRepository
{
    private readonly IMongoCollection<FavoriteProduct> _favorites = mongoDbContext.FavoriteProducts;

    public async Task CreateAsync(FavoriteProduct favoriteProduct)
    {
        await _favorites.InsertOneAsync(favoriteProduct);
    }

    public async Task<bool> DeleteAsync(ObjectId id)
    {
        var filter = Builders<FavoriteProduct>.Filter.Eq(f => f.Id, id);
        return (await _favorites.DeleteOneAsync(filter)).DeletedCount > 0;
    }

    public async Task<bool> UpdateAsync(FavoriteProduct favoriteProduct)
    {
        var filter = Builders<FavoriteProduct>.Filter.Eq(f => f.Id, favoriteProduct.Id);
        return (await _favorites.ReplaceOneAsync(filter, favoriteProduct)).MatchedCount > 0;
    }

    public async Task<List<FavoriteProduct>?> GetByNameAsync(string name)
    {
        return await _favorites.Find(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToListAsync();
    }

    public async Task<FavoriteProduct?> GetAsync(ObjectId id)
    {
        var filter = Builders<FavoriteProduct>.Filter.Eq(f => f.Id, id);
        return await _favorites.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<FavoriteProduct>?> GetAllAsync()
    {
        var filter = Builders<FavoriteProduct>.Filter.Empty;
        return await _favorites.Find(filter).ToListAsync();
    }

    public async Task<List<FavoriteProduct>?> GetAllByUserIdAsync(ObjectId userId)
    {
        var filter = Builders<FavoriteProduct>.Filter.Eq(f => f.UserId, userId);
        return await _favorites.Find(filter).ToListAsync();
    }

    public async Task<bool> DeleteAllByUserIdAsync(ObjectId userId)
    {
        var filter = Builders<FavoriteProduct>.Filter.Eq(f => f.UserId, userId);
        return (await _favorites.DeleteOneAsync(filter)).DeletedCount > 0;
    }
}