using App.Core.DTOs.Sell;
using App.Core.Enums;
using App.Core.Models.Sell;

namespace App.Core.Interfaces;

public interface IOrderService
{
    Task BuyRegistered(string userId, DeliveryPayment deliveryPayment, PointsOfDelivery deliveryTo,
        string? phoneNumber, string? firstName, string? lastName, string? middleName);

    Task<IEnumerable<OrderDto>> GetByUserId(string userId);
    Task<IEnumerable<OrderDto>> GetByStoreNeedToAccept(string userId);
    Task<IEnumerable<OrderDto>> GetByStoreAccepted(string userId);
    Task RejectOrder(string userId, string orderId, string reason);
    Task AcceptOrder(string userId, string buyInfoId);
}