using App.Core.Models.Sell;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IBuyInfoRepository
{
    Task CreateAsync(BuyInfo buyInfo);
    Task<bool> UpdateAsync(BuyInfo buyInfo);
    Task<BuyInfo> GetByIdAsync(ObjectId id);
    Task<List<BuyInfo>> GetByUserIdAsync(ObjectId userId);
    Task<List<BuyInfo>> GetAllAsync();
    Task<List<BuyInfo>> GetBySellerId(ObjectId sellerId);
    Task<List<BuyInfo>> GetByProductId(ObjectId productId);
    Task<BuyInfo> GetByTrackingNumber(string trackingNumber);
}