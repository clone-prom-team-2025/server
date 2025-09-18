using App.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Sell;

public class BuyInfo
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    public ObjectId SellerId { get; set; }

    public string? TrackingNumber { get; set; }
    
    public ObjectId UserId { get; set; }
    
    public PointsOfDelivery? DeliveryToInfo { get; set; }
    
    public MiniProductInfo MiniProductInfo { get; set; } = null!;

    public bool? Payed { get; set; } = null;
    
    public DeliveryPayment Payment { get; set; }

    public DeliveryStatus Status { get; set; } = DeliveryStatus.AwaitingConfirmation;
}