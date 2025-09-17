using App.Core.Enums;
using App.Core.Models.Sell;

namespace App.Core.DTOs.Sell;

public class BuyCreateDto
{
    public string ProductId { get; set; } = null!;
    public DeliveryPayment DeliveryPayment  { get; set; }
    public PointsOfDelivery DeliveryTo { get; set; } = null!;
}