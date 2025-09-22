using App.Core.Enums;
using App.Core.Models.FileStorage;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Sell;

public class Order
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    public string OrderNumber { get; set; }

    public ObjectId SellerId { get; set; }

    public string? TrackingNumber { get; set; }
    
    public ObjectId? UserId { get; set; }

    public string FirstName { get; set; } = null!;
    
    public string LastName { get; set; } = null!;
    
    public string? MiddleName { get; set; }

    public string PhoneNumber { get; set; } = null!;
    
    public string Email { get; set; } = null!;

    public PointsOfDelivery DeliveryToInfo { get; set; } = new();
    
    public int Pcs { get; set; }
    
    public decimal TotalPrice { get; set; } = 0;

    public MiniProductInfo MiniProductsInfo { get; set; } = new();

    public bool? Payed { get; set; } = null;
    
    public DateTime CreatedAt { get; set; }
    
    public DeliveryPayment Payment { get; set; }
    
    public bool Confirmed { get; set; } = false;

    public bool Registered = false;
    
    public string? SellerMessage { get; set; }

    public DeliveryStatus Status { get; set; } = DeliveryStatus.AwaitingConfirmation;
}