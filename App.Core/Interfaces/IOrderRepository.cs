using App.Core.Models.Sell;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IOrderRepository
{
    Task CreateAsync(Order order);
    Task CreateManyAsync(IEnumerable<Order> buyInfos);
    Task<bool> UpdateAsync(Order order);
    Task<Order?> GetByIdAsync(ObjectId id);
    Task<List<Order>?> GetByOrderNumberAsync(string orderNumber);
    Task<List<Order>?> GetByUserIdAsync(ObjectId userId);
    Task<List<Order>?> GetAllAsync();
    Task<List<Order>?> GetBySellerId(ObjectId sellerId);
    Task<List<Order>?> GetByProductId(ObjectId productId);
    Task<Order?> GetByTrackingNumber(string trackingNumber);
    Task<bool> DeleteAsync(ObjectId id);
}