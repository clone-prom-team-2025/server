using App.Core.Models.Cart;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface ICartRepository
{
    Task<bool> CreateAsync(Cart cart);
    Task<bool> UpdateAsync(Cart cart);
    Task<bool> DeleteAsync(ObjectId id, ObjectId userId);
    Task<List<Cart>?> GetAllAsync();
    Task<Cart?> GetByIdAsync(ObjectId id);
    Task<List<Cart>?> GetByUserIdAsync(ObjectId userId);
    Task<bool> DeleteByUserIdAsync(ObjectId userId);
    Task<bool> ExistsAsync(ObjectId userId, ObjectId productId);
}