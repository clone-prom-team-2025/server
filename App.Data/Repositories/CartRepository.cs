using App.Core.Interfaces;
using App.Core.Models.Cart;
using App.Core.Models.Product;
using App.Core.Models.User;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class CartRepository(MongoDbContext mongoDbContext, ILogger<CartRepository> logger) : ICartRepository
{
    private readonly IMongoCollection<Cart> _carts = mongoDbContext.Carts;
    private readonly ILogger<CartRepository> _logger = logger;
    private readonly IMongoCollection<Product> _products = mongoDbContext.Products;
    private readonly IMongoCollection<User> _users = mongoDbContext.Users;

    public async Task<bool> CreateAsync(Cart cart)
    {
        _logger.LogInformation("CreateAsync called for ProductId={productId} by UserId={userId}", cart.ProductId,
            cart.UserId);
        var productFilter = Builders<Product>.Filter.Eq(x => x.Id, cart.ProductId);
        if (!await _products.Find(productFilter).AnyAsync())
        {
            _logger.LogError("CreateAsync failed: product not found for Id={productId}", cart.ProductId);
            return false;
        }

        var userFilter = Builders<User>.Filter.Eq(x => x.Id, cart.UserId);
        if (!await _users.Find(userFilter).AnyAsync())
        {
            _logger.LogError("CreaCreateAsyncte failed: user not found for UserId={userId}", cart.UserId);
            return false;
        }

        var cartFilter = Builders<Cart>.Filter.And(
            Builders<Cart>.Filter.Eq(x => x.ProductId, cart.ProductId),
            Builders<Cart>.Filter.Eq(x => x.UserId, cart.UserId)
        );
        if (await _carts.Find(cartFilter).AnyAsync())
        {
            _logger.LogError("CreateAsync failed: cart already exists");
            return false;
        }

        _logger.LogDebug("ProductId: {id}", cart.ProductId);
        await _carts.InsertOneAsync(cart);
        return true;
    }

    public async Task<bool> UpdateAsync(Cart cart)
    {
        _logger.LogInformation("UpdateAsync called for ProductId={productId} by UserId={userId}", cart.ProductId,
            cart.UserId);
        var productFilter = Builders<Product>.Filter.Eq(x => x.Id, cart.ProductId);
        if (!await _products.Find(productFilter).AnyAsync())
        {
            _logger.LogError("UpdateAsync failed: product not found for Id={productId}", cart.ProductId);
            return false;
        }

        var userFilter = Builders<User>.Filter.Eq(x => x.Id, cart.UserId);
        if (!await _users.Find(userFilter).AnyAsync())
        {
            _logger.LogError("UpdateAsync failed: user not found for UserId={userId}", cart.UserId);
            return false;
        }

        var filter = Builders<Cart>.Filter.Eq(c => c.Id, cart.Id);
        var result = await _carts.ReplaceOneAsync(filter, cart);
        _logger.LogInformation("UpdateAsync result for CartId={id}: {IsAcknowledged}", cart.Id, result.IsAcknowledged);
        return result.IsAcknowledged;
    }

    public async Task<bool> DeleteAsync(ObjectId id, ObjectId userId)
    {
        _logger.LogInformation("DeleteAsync called for CartId={cartId}", id);
        var filter = Builders<Cart>.Filter.And(
            Builders<Cart>.Filter.Eq(c => c.Id, id),
            Builders<Cart>.Filter.Eq(c => c.UserId, userId)
        );

        var result = await _carts.DeleteOneAsync(filter);

        if (!result.IsAcknowledged || result.DeletedCount == 0)
        {
            _logger.LogWarning(
                "DeleteAsync failed: cart not found or does not belong to user. CartId={cartId}, UserId={userId}", id,
                userId);
            return false;
        }

        _logger.LogInformation("DeleteAsync succeeded for CartId={cartId}", id);
        return true;
    }

    public async Task<List<Cart>?> GetAllAsync()
    {
        _logger.LogInformation("GetAllAsync called");
        var filter = Builders<Cart>.Filter.Empty;
        var result = await _carts.Find(filter).ToListAsync();
        _logger.LogInformation("GetAllAsync returned {Count} carts", result.Count);
        return await _carts.Find(filter).ToListAsync();
    }

    public async Task<Cart?> GetByIdAsync(ObjectId id)
    {
        _logger.LogInformation("GetByIdAsync called for CartId={cartId}", id);
        var filter = Builders<Cart>.Filter.Eq(c => c.Id, id);
        var result = await _carts.Find(filter).FirstOrDefaultAsync();
        if (result == null)
        {
            _logger.LogError("GetByIdAsync failed: Cart not found for Id={cartId}", id);
            return null;
        }

        _logger.LogInformation("GetByIdAsync result: {@Cart}", result);
        return result;
    }

    public async Task<List<Cart>?> GetByUserIdAsync(ObjectId userId)
    {
        _logger.LogInformation("GetByUserIdAsync called for UserId={UserId}", userId);
        var filter = Builders<Cart>.Filter.Eq(c => c.UserId, userId);
        var result = await _carts.Find(filter).ToListAsync();
        _logger.LogInformation("GetByUserIdAsync returned {Count} carts for UserId={UserId}", result.Count, userId);
        return result;
    }

    public async Task<bool> DeleteByUserIdAsync(ObjectId userId)
    {
        _logger.LogInformation("DeleteByUserIdAsync called for UserId={cartId}", userId);
        var filter = Builders<Cart>.Filter.Eq(c => c.UserId, userId);

        var result = await _carts.DeleteOneAsync(filter);

        if (!result.IsAcknowledged || result.DeletedCount == 0)
        {
            _logger.LogWarning("DeleteByUserIdAsync failed: cart not found. UserId={userId}", userId);
            return false;
        }

        _logger.LogInformation("DeleteByUserIdAsync succeeded for UserId={userId}", userId);
        return true;
    }

    public async Task<bool> ExistsAsync(ObjectId userId, ObjectId productId)
    {
        _logger.LogInformation("ExistsAsync called for UserId={cartId}, ProductId={productId}", userId, productId);
        var filter = Builders<Cart>.Filter.And(
            Builders<Cart>.Filter.Eq(c => c.UserId, userId),
            Builders<Cart>.Filter.Eq(c => c.ProductId, productId)
        );

        var result = await _carts.Find(filter).AnyAsync();
        _logger.LogInformation("ExistsAsync result: {res}", result);
        return result;
    }
}