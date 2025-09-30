using App.Core.Enums;
using App.Core.Models.Sell;

namespace App.Core.DTOs.Sell;

public class BuyUnRegisteredRequest
{
    public Dictionary<string, int> Products { get; set; } = new();
    public DeliveryPayment DeliveryPayment { get; set; }
    public PointsOfDelivery DeliveryTo { get; set; }
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
}