using App.Core.Interfaces;
using App.Core.Models.Sell;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class OrderRepository(MongoDbContext context) : IOrderRepository
{
    private readonly IMongoCollection<Order> _buyInfos = context.Orders;

    public async Task CreateAsync(Order order)
    {
        await _buyInfos.InsertOneAsync(order);
    }

    public async Task CreateManyAsync(IEnumerable<Order> buyInfos)
    {
        await _buyInfos.InsertManyAsync(buyInfos);
    }

    public async Task<bool> UpdateAsync(Order order)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.Id, order.Id);
        return (await _buyInfos.ReplaceOneAsync(filter, order)).MatchedCount > 0;
    }

    public async Task<Order?> GetByIdAsync(ObjectId id)
    {
        var filter = Builders<Order>.Filter.Eq(d =>d.Id, id);
        return await _buyInfos.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<Order>?> GetByOrderNumberAsync(string orderNumber)
    {
        var filter = Builders<Order>.Filter.Eq(d => d.OrderNumber, orderNumber);
        return await _buyInfos.Find(filter).ToListAsync();
    }

    public async Task<List<Order>?> GetByUserIdAsync(ObjectId userId)
    {
        var filter = Builders<Order>.Filter.Eq(d => d.UserId, userId);
        return await  _buyInfos.Find(filter).ToListAsync();
    }

    public async Task<List<Order>?> GetAllAsync()
    {
        var filter = Builders<Order>.Filter.Empty;
        return await _buyInfos.Find(filter).ToListAsync();
    }

    public async Task<List<Order>?> GetBySellerId(ObjectId sellerId)
    {
        var filter = Builders<Order>.Filter.Eq(d => d.SellerId, sellerId);
        return await _buyInfos.Find(filter).ToListAsync();
    }

    public async Task<List<Order>?> GetByEmailAsync(string email)
    {
        var filter = Builders<Order>.Filter.Eq(d => d.Email, email);
        return await _buyInfos.Find(filter).ToListAsync();
    }

    public async Task<List<Order>?> GetByProductId(ObjectId productId)
    {
        var filter = Builders<Order>.Filter.Eq(d => d.Id, productId);
        return await _buyInfos.Find(filter).ToListAsync();
    }

    public async Task<Order?> GetByTrackingNumber(string trackingNumber)
    {
        var filter = Builders<Order>.Filter.Eq(d => d.TrackingNumber, trackingNumber);
        return await _buyInfos.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteAsync(ObjectId id)
    {
        var filter = Builders<Order>.Filter.Eq(d => d.Id, id);
        return (await _buyInfos.DeleteOneAsync(filter)).DeletedCount > 0;
    }
}