using App.Core.Enums;

namespace App.Core.DTOs.Sell;

public class DeliveryAndPaymentDto
{
    public ProductDeliveryType ProductDeliveryType { get; set; }
    public PaymentOptions PaymentOptions { get; set; }
}