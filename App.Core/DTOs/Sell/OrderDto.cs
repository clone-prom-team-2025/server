using App.Core.Enums;
using App.Core.Models.FileStorage;
using App.Core.Models.Sell;

namespace App.Core.DTOs.Sell;

public class OrderDto
{
    public string Id { get; set; }

    public string SellerId { get; set; }

    public string? TrackingNumber { get; set; }
    
    public string? UserId { get; set; }

    public string FirstName { get; set; } = null!;
    
    public string LastName { get; set; } = null!;
    
    public string? MiddleName { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public PointsOfDelivery DeliveryToInfo { get; set; } = new();
    
    public int Pcs { get; set; }
    
    public decimal TotalPrice { get; set; } = 0;

    public MiniProductInfo MiniProductsInfo { get; set; } = new();

    public bool? Payed { get; set; } = null;
    
    public DeliveryPayment Payment { get; set; }
    
    public bool Confirmed { get; set; } = false;

    public DeliveryStatus Status { get; set; } = DeliveryStatus.AwaitingConfirmation;
}