using App.Core.Enums;
using App.Core.Models.Sell;

namespace App.Core.DTOs.Product;

public class ProductCreateDto
{
    public ProductCreateDto(string name, string category, List<ProductFeatureDto> features, int quantity, QuantityStatus quantityStatus, decimal price, PriceType priceType,
        PaymentOptions paymentOptions, ProductDeliveryType deliveryType, decimal? discountPrice = null)
    {
        Name = name;
        Category = category;
        Features = [..features];
        Price = price;
        DiscountPrice = discountPrice;
        QuantityStatus = quantityStatus;
        Quantity = quantity;
        PriceType = priceType;
        PaymentOptions = paymentOptions;
        DeliveryType = deliveryType;
    }

    public ProductCreateDto()
    {
    }

    public string Name { get; set; }

    public string Category { get; set; }

    public List<ProductFeatureDto> Features { get; set; }

    public decimal Price { get; set; }

    public PriceType PriceType { get; set; }

    public PaymentOptions PaymentOptions { get; set; }

    public decimal? DiscountPrice { get; set; }
    
    public QuantityStatus QuantityStatus { get; set; }

    public int Quantity { get; set; }

    public ProductDeliveryType DeliveryType { get; set; }
    
    public ProductDimensions ProductDimensions { get; set; } = new ProductDimensions();
}