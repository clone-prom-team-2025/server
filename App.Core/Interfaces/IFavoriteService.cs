using App.Core.DTOs.Favorite;

namespace App.Core.Interfaces;

public interface IFavoriteService
{
    Task CreateFavoriteProductCollection(FavoriteProductCreateDto dto);
    Task<IEnumerable<FavoriteProductDto>?> GetFavoriteProductAllByUserIdAsync(string userId);
    Task UpdateFavoriteProductCollectionName(string id, string name, string userId);
    Task<IEnumerable<FavoriteProductDto>?> GetFavoriteProductAllByIdAsync(string id);
    Task AddToFavoriteProductCollection(string id, string userId, string productId);
    Task AddToFavoriteProductCollectionByName(string name, string userId, string productId);
    Task AddToFavoriteProductCollectionToDefault(string userId, string productId);
    Task RemoveFromFavoriteProductCollection(string id, string userId, string productId);
    Task CreateEmptyFavoriteProductCollection(string name, string userId);
    Task CreateDefaultFavoriteProductCollectionIfNotExist(string userId);
    Task DeleteFavoriteProductCollection(string id, string userId);


    Task CreateFavoriteSellerCollection(FavoriteSellerCreateDto dto);
    Task<IEnumerable<FavoriteSellerDto>?> GetFavoriteSellerAllByUserIdAsync(string userId);
    Task UpdateFavoriteSellerCollectionName(string id, string name, string userId);
    Task<IEnumerable<FavoriteSellerDto>?> GetFavoriteSellerAllByIdAsync(string id);
    Task AddToFavoriteSellerCollection(string id, string userId, string sellerId);
    Task AddToFavoriteSellerCollectionByName(string name, string userId, string productSellerIdtId);
    Task AddToFavoriteSellerCollectionToDefault(string userId, string sellerId);
    Task RemoveFromFavoriteSellerCollection(string id, string userId, string sellerId);
    Task CreateEmptyFavoriteSellerCollection(string name, string userId);
    Task CreateDefaultFavoriteSellerCollectionIfNotExist(string userId);
    Task DeleteFavoriteSellerCollection(string id, string userId);
}