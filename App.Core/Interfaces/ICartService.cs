using App.Core.DTOs.Cart;

namespace App.Core.Interfaces;

public interface ICartService
{
    Task<bool> AddAsync(CreateCartDto dto, string userId);
    Task<bool> RemoveAsync(string id, string userId);
    Task<bool> ClearAsync(string userId);
    Task<bool> ChangePcsAsync(string id, int pcs, string userId);
    Task<IEnumerable<CartDto>?> GetAllAsync();
    Task<CartDto?> GetByIdAsync(string id);
    Task<IEnumerable<CartDto>?> GetByUserIdAsync(string userId);
    Task<bool> IsProductInCartAsync(string productId, string userId);
}