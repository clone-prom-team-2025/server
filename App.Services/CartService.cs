using App.Core.DTOs.Cart;
using App.Core.Interfaces;
using App.Core.Models.Cart;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services;

public class CartService(ICartRepository cartRepository, IMapper mapper, ILogger<CartService> logger) : ICartService
{
    private readonly ICartRepository _cartRepository = cartRepository;
    private readonly ILogger<CartService> _logger = logger;
    private readonly IMapper _mapper = mapper;

    public async Task AddAsync(CreateCartDto dto, string userId)
    {
        using (_logger.BeginScope("AddAsync: ProductId={productId}, UserId={userId}", dto.ProductId, userId))
        {
            _logger.LogInformation("AddAsync called with ProductId={productId}, Pcs={pcs}, UserId={userId}",
                dto.ProductId, dto.Pcs, userId);
            var model = new Cart
            {
                Id = ObjectId.GenerateNewId(),
                ProductId = ObjectId.Parse(dto.ProductId),
                Pcs = dto.Pcs,
                UserId = ObjectId.Parse(userId),
                CreatedAt = DateTime.UtcNow
            };
            var result = await _cartRepository.CreateAsync(model);
            if (!result)
            {
                _logger.LogError("AddAsync: Failed to create cart");
                throw new Exception("Failed to create cart");
            }

            _logger.LogInformation("AddAsync: Successfully created cart");
        }
    }

    public async Task RemoveAsync(string id, string userId)
    {
        using (_logger.BeginScope("RemoveAsync: id={id}, UserId={userId}", id, userId))
        {
            _logger.LogInformation("RemoveAsync called with id={id}, UserId={userId}", id, userId);
            var result = await _cartRepository.DeleteAsync(ObjectId.Parse(id), ObjectId.Parse(userId));
            if (!result)
            {
                _logger.LogError("RemoveAsync: Failed to delete cart");
                throw new Exception("Failed to delete cart");
            }

            _logger.LogInformation("RemoveAsync: Successfully deleted cart");
        }
    }

    public async Task ClearAsync(string userId)
    {
        using (_logger.BeginScope("RemoveAsync: UserId={userId}", userId))
        {
            _logger.LogInformation("RemoveAsync called with UserId={userId}", userId);
            var result = await _cartRepository.DeleteByUserIdAsync(ObjectId.Parse(userId));
            if (!result)
            {
                _logger.LogError("RemoveAsync: Failed to delete carts");
                throw new Exception("Failed to delete carts");
            }

            _logger.LogInformation("RemoveAsync: Successfully deleted carts");
        }
    }

    public async Task ChangePcsAsync(string id, int pcs, string userId)
    {
        using (_logger.BeginScope("ChangePcsAsync: Id={id}, Pcs={pcs}, UserId={userId}", id, pcs, userId))
        {
            _logger.LogInformation("ChangePcsAsync called with Id={id}, Pcs={pcs}, UserId={userId}", id, pcs, userId);
            var cart = await _cartRepository.GetByIdAsync(ObjectId.Parse(id));
            if (cart == null)
            {
                _logger.LogError("ChangePcsAsync: Failed to find cart. Cart not found");
                throw new Exception("Cart not found");
            }

            if (cart.UserId.ToString() != userId)
            {
                _logger.LogError("ChangePcsAsync: Failed to find cart. It's not your cart!");
                throw new Exception("It's not your cart");
            }

            if (pcs <= 0)
            {
                _logger.LogInformation("ChangePcsAsync: Pcs is equal or less than 0, deleting cart");
                var resultDelete = await _cartRepository.DeleteAsync(ObjectId.Parse(id), ObjectId.Parse(userId));
                if (!resultDelete)
                {
                    _logger.LogError("ChangePcsAsync: Failed to update cart");
                    throw new Exception("Failed to update cart");
                }

                _logger.LogInformation("ChangePcsAsync: Successfully updated cart(Removed)");
            }

            cart.Pcs = pcs;
            var result = await _cartRepository.UpdateAsync(cart);
            if (!result)
            {
                _logger.LogError("ChangePcsAsync: Failed to update cart");
                throw new Exception("Failed to update cart");
            }

            _logger.LogInformation("ChangePcsAsync: Successfully updated cart");
        }
    }

    public async Task<IEnumerable<CartDto>?> GetAllAsync()
    {
        using (_logger.BeginScope("GetAllAsync"))
        {
            _logger.LogInformation("GetAllAsync called");
            var carts = await _cartRepository.GetAllAsync();
            if (carts == null || !carts.Any())
            {
                _logger.LogInformation("GetAllAsync: no carts found");
                return null;
            }

            _logger.LogInformation("GetAllAsync: Successfully retrieved carts");
            return _mapper.Map<IEnumerable<CartDto>?>(carts);
        }
    }

    public async Task<CartDto?> GetByIdAsync(string id)
    {
        using (_logger.BeginScope("GetByIdAsync: CartId={id}", id))
        {
            _logger.LogInformation("GetByIdAsync called with Id={id}", id);
            var cart = await _cartRepository.GetByIdAsync(ObjectId.Parse(id));
            if (cart == null)
            {
                _logger.LogError("GetByIdAsync: Failed to find cart. Cart not found");
                return null;
            }

            _logger.LogInformation("GetByIdAsync: Successfully retrieved carts");
            return _mapper.Map<CartDto?>(cart);
        }
    }

    public async Task<IEnumerable<CartDto>?> GetByUserIdAsync(string userId)
    {
        using (_logger.BeginScope("GetByUserIdAsync: UserId={id}", userId))
        {
            _logger.LogInformation("GetByUserIdAsync called with Id={id}", userId);
            var carts = await _cartRepository.GetByUserIdAsync(ObjectId.Parse(userId));
            if (carts == null || !carts.Any())
            {
                _logger.LogInformation("GetByUserIdAsync: no carts found");
                return null;
            }

            _logger.LogInformation("GetByIdAsync: Successfully retrieved carts");
            return _mapper.Map<IEnumerable<CartDto>?>(carts);
        }
    }

    public async Task<bool> IsProductInCartAsync(string productId, string userId)
    {
        using (_logger.BeginScope("GetByIdAsync: ProductId={productId}, UserId={userId}", productId, userId))
        {
            _logger.LogInformation("GetByIdAsync called with ProductId={productId}, UserId={userId}", productId,
                userId);
            var result = await _cartRepository.ExistsAsync(ObjectId.Parse(userId), ObjectId.Parse(productId));
            if (!result) _logger.LogInformation("GetByIdAsync: Product not in UserId={userId} cart", userId);
            return result;
        }
    }
}