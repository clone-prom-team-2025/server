using App.Core.DTOs.Cart;

namespace App.Core.Interfaces;

public interface ICartService
{
    Task AddAsync(CreateCartDto dto, string userId);
    Task RemoveAsync(string id, string userId);
    Task ClearAsync(string userId);
    Task ChangePcsAsync(string id, int pcs, string userId);
    Task<IEnumerable<CartDto>?> GetAllAsync();
    Task<CartDto?> GetByIdAsync(string id);
    Task<IEnumerable<CartDto>?> GetByUserIdAsync(string userId);
    Task<bool> IsProductInCartAsync(string productId, string userId);
}