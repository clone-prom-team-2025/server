using App.Core.Interfaces;
using App.Core.Models.Sell;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class BuyInfoRepository(MongoDbContext context) : IBuyInfoRepository
{
    private readonly IMongoCollection<BuyInfo> _deliveryInfos = context.BuyInfos;

    public async Task CreateAsync(BuyInfo buyInfo)
    {
        await _deliveryInfos.InsertOneAsync(buyInfo);
    }

    public async Task<bool> UpdateAsync(BuyInfo buyInfo)
    {
        var filter = Builders<BuyInfo>.Filter.Eq(x => x.Id, buyInfo.Id);
        return (await _deliveryInfos.ReplaceOneAsync(filter, buyInfo)).MatchedCount > 0;
    }

    public async Task<BuyInfo?> GetByIdAsync(ObjectId id)
    {
        var filter = Builders<BuyInfo>.Filter.Eq(d =>d.Id, id);
        return await _deliveryInfos.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<BuyInfo>?> GetByUserIdAsync(ObjectId userId)
    {
        var filter = Builders<BuyInfo>.Filter.Eq(d => d.UserId, userId);
        return await  _deliveryInfos.Find(filter).ToListAsync();
    }

    public async Task<List<BuyInfo>?> GetAllAsync()
    {
        var filter = Builders<BuyInfo>.Filter.Empty;
        return await _deliveryInfos.Find(filter).ToListAsync();
    }

    public async Task<List<BuyInfo>?> GetBySellerId(ObjectId sellerId)
    {
        var filter = Builders<BuyInfo>.Filter.Eq(d => d.SellerId, sellerId);
        return await _deliveryInfos.Find(filter).ToListAsync();
    }

    public async Task<List<BuyInfo>?> GetByProductId(ObjectId productId)
    {
        var filter = Builders<BuyInfo>.Filter.Eq(d => d.Id, productId);
        return await _deliveryInfos.Find(filter).ToListAsync();
    }

    public async Task<BuyInfo?> GetByTrackingNumber(string trackingNumber)
    {
        var filter = Builders<BuyInfo>.Filter.Eq(d => d.TrackingNumber, trackingNumber);
        return await _deliveryInfos.Find(filter).FirstOrDefaultAsync();
    }
}