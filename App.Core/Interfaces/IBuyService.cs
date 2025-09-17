using App.Core.DTOs.Sell;
using App.Core.Enums;
using App.Core.Models.Sell;

namespace App.Core.Interfaces;

public interface IBuyService
{
    Task BuyProductAsync(BuyCreateDto dto, string userId);
    Task AcceptSellAsync(string buyId, string userId);
    Task PayForProductAsync(string buyId, string userId);
    Task SendProductAsync(string buyId, string userId);
    Task CancelSellAsync(string buyId, string userId);
    Task DeclineSellAsync(string buyId, string userId);
    Task<BuyInfoDto> GetBuyInfoAsync(string buyId);
    Task<IEnumerable<BuyInfoDto>> GetBuyInfoByUserIdAsync(string userId);
    Task<IEnumerable<BuyInfoDto>> GetBuyInfoBySellerIdAsync(string sellerId);
}