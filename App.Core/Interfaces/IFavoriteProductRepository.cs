using App.Core.Models.Favorite;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IFavoriteProductRepository
{
    Task CreateAsync(FavoriteProduct favoriteProduct);
    Task<bool> DeleteAsync(ObjectId id);
    Task<bool> UpdateAsync(FavoriteProduct favoriteProduct);
    Task<List<FavoriteProduct>?> GetByNameAsync(string name);
    Task<FavoriteProduct?> GetAsync(ObjectId id);
    Task<List<FavoriteProduct>?> GetAllAsync();
    Task<List<FavoriteProduct>?> GetAllByUserIdAsync(ObjectId userId);
    Task<bool> DeleteAllByUserIdAsync(ObjectId userId);
}