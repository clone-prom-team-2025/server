using App.Core.Enums;
using App.Core.Models.Sell;

namespace App.Core.DTOs.Sell;

public class BuyInfoDto
{
    public string Id { get; set; }
    
    public string SellerId { get; set; }

    public string? TrackingNumber { get; set; }
    
    public string UserId { get; set; }
    
    public PointsOfDelivery? DeliveryToInfo { get; set; }
    
    public MiniProductInfoDto MiniProductInfo { get; set; } = null!;

    public bool? Payed { get; set; } = null;
    
    public DeliveryPayment Payment { get; set; }

    public DeliveryStatus Status { get; set; } = DeliveryStatus.AwaitingConfirmation;
}