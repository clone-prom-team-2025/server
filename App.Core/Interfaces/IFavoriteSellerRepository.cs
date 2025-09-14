using App.Core.Models.Favorite;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IFavoriteSellerRepository
{
    Task CreateAsync(FavoriteSeller favoriteSeller);
    Task<bool> DeleteAsync(ObjectId id);
    Task<bool> UpdateAsync(FavoriteSeller favoriteSeller);
    Task<List<FavoriteSeller>?> GetByNameAsync(string name);
    Task<FavoriteSeller?> GetAsync(ObjectId id);
    Task<List<FavoriteSeller>?> GetAllAsync();
    Task<List<FavoriteSeller>?> GetAllByUserIdAsync(ObjectId userId);
    Task<bool> DeleteAllByUserIdAsync(ObjectId userId);
}