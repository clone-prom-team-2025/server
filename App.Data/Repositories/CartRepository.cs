using App.Core.Interfaces;
using App.Core.Models.Cart;
using App.Core.Models.Product;
using App.Core.Models.User;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class CartRepository(MongoDbContext mongoDbContext) : ICartRepository
{
    private readonly IMongoCollection<Cart> _carts = mongoDbContext.Carts;
    private readonly IMongoCollection<Product> _products = mongoDbContext.Products;
    private readonly IMongoCollection<User> _users = mongoDbContext.Users;

    public async Task<bool> CreateAsync(Cart cart)
    {
        var productFilter = Builders<Product>.Filter.Eq(x => x.Id, cart.ProductId);
        if (!await _products.Find(productFilter).AnyAsync()) return false;

        var userFilter = Builders<User>.Filter.Eq(x => x.Id, cart.UserId);
        if (!await _users.Find(userFilter).AnyAsync()) return false;

        var cartFilter = Builders<Cart>.Filter.And(
            Builders<Cart>.Filter.Eq(x => x.ProductId, cart.ProductId),
            Builders<Cart>.Filter.Eq(x => x.UserId, cart.UserId)
        );
        if (await _carts.Find(cartFilter).AnyAsync()) return false;

        await _carts.InsertOneAsync(cart);
        return true;
    }

    public async Task<bool> UpdateAsync(Cart cart)
    {
        var productFilter = Builders<Product>.Filter.Eq(x => x.Id, cart.ProductId);
        if (!await _products.Find(productFilter).AnyAsync()) return false;

        var userFilter = Builders<User>.Filter.Eq(x => x.Id, cart.UserId);
        if (!await _users.Find(userFilter).AnyAsync()) return false;

        var filter = Builders<Cart>.Filter.Eq(c => c.Id, cart.Id);
        var result = await _carts.ReplaceOneAsync(filter, cart);
        return result.IsAcknowledged;
    }

    public async Task<bool> DeleteAsync(ObjectId id, ObjectId userId)
    {
        var filter = Builders<Cart>.Filter.And(
            Builders<Cart>.Filter.Eq(c => c.Id, id),
            Builders<Cart>.Filter.Eq(c => c.UserId, userId)
        );

        var result = await _carts.DeleteOneAsync(filter);

        if (!result.IsAcknowledged || result.DeletedCount == 0) return false;

        return true;
    }

    public async Task<List<Cart>?> GetAllAsync()
    {
        var filter = Builders<Cart>.Filter.Empty;
        return await _carts.Find(filter).ToListAsync();
    }

    public async Task<Cart?> GetByIdAsync(ObjectId id)
    {
        var filter = Builders<Cart>.Filter.Eq(c => c.Id, id);
        var result = await _carts.Find(filter).FirstOrDefaultAsync();
        if (result == null) return null;

        return result;
    }

    public async Task<List<Cart>?> GetByUserIdAsync(ObjectId userId)
    {
        var filter = Builders<Cart>.Filter.Eq(c => c.UserId, userId);
        var result = await _carts.Find(filter).ToListAsync();
        return result;
    }

    public async Task<bool> DeleteByUserIdAsync(ObjectId userId)
    {
        var filter = Builders<Cart>.Filter.Eq(c => c.UserId, userId);

        var result = await _carts.DeleteOneAsync(filter);

        if (!result.IsAcknowledged || result.DeletedCount == 0) return false;

        return true;
    }

    public async Task<bool> ExistsAsync(ObjectId userId, ObjectId productId)
    {
        var filter = Builders<Cart>.Filter.And(
            Builders<Cart>.Filter.Eq(c => c.UserId, userId),
            Builders<Cart>.Filter.Eq(c => c.ProductId, productId)
        );

        var result = await _carts.Find(filter).AnyAsync();
        return result;
    }
}