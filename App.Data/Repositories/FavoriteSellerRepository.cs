using App.Core.Interfaces;
using App.Core.Models.Favorite;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class FavoriteSellerRepository(MongoDbContext mongoDbContext) : IFavoriteSellerRepository
{
    private readonly IMongoCollection<FavoriteSeller> _favoriteSellers =  mongoDbContext.FavoriteSellers;
    
    public async Task CreateAsync(FavoriteSeller favoriteSeller)
    {
        await  _favoriteSellers.InsertOneAsync(favoriteSeller);
    }

    public async Task<bool> DeleteAsync(ObjectId id)
    {
        var filter = Builders<FavoriteSeller>.Filter.Eq(x => x.Id, id);
        return (await _favoriteSellers.DeleteOneAsync(filter)).DeletedCount > 0;
    }

    public async Task<bool> UpdateAsync(FavoriteSeller favoriteSeller)
    {
        var filter = Builders<FavoriteSeller>.Filter.Eq(x => x.Id, favoriteSeller.Id);
        return (await _favoriteSellers.ReplaceOneAsync(filter, favoriteSeller)).MatchedCount > 0;
    }

    public async Task<List<FavoriteSeller>?> GetByNameAsync(string name)
    {
        return await _favoriteSellers.Find(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToListAsync();
    }

    public async Task<FavoriteSeller?> GetAsync(ObjectId id)
    {
        var filter = Builders<FavoriteSeller>.Filter.Eq(x => x.Id, id);
        return await _favoriteSellers.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<FavoriteSeller>?> GetAllAsync()
    {
        var filter = Builders<FavoriteSeller>.Filter.Empty;
        return await _favoriteSellers.Find(filter).ToListAsync();
    }

    public async Task<List<FavoriteSeller>?> GetAllByUserIdAsync(ObjectId userId)
    {
        var filter = Builders<FavoriteSeller>.Filter.Eq(x => x.UserId, userId);
        return await _favoriteSellers.Find(filter).ToListAsync();
    }

    public async Task<bool> DeleteAllByUserIdAsync(ObjectId userId)
    {
        var filter  = Builders<FavoriteSeller>.Filter.Eq(x => x.UserId, userId);
        return (await _favoriteSellers.DeleteOneAsync(filter)).DeletedCount > 0;
    }
}