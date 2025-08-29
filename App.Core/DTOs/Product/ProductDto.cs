using App.Core.Enums;

namespace App.Core.DTOs.Product;

public class ProductDto
{
    public ProductDto(string id, string name, List<string> categoryPath, List<ProductFeatureDto> features,
        string sellerId, int quantity, QuantityStatus quantityStatus, decimal price, PriceType priceType, PaymentOptions paymentOptions, ProductDeliveryType deliveryType ,decimal? discountPrice = null)
    {
        Id = id;
        Name = name;
        CategoryPath = new List<string>(categoryPath);
        Features = [..features];
        SellerId = sellerId;
        Price = price;
        DiscountPrice = discountPrice;
        QuantityStatus = quantityStatus;
        Quantity = quantity;
        PriceType = priceType;
        PaymentOptions = paymentOptions;
        DeliveryType = deliveryType;
    }

    public ProductDto()
    {
    }

    public string Id { get; set; }

    public string Name { get; set; }

    public List<string> CategoryPath { get; set; }

    public List<ProductFeatureDto> Features { get; set; }

    public decimal Price { get; set; }
    
    public PriceType PriceType { get; set; }
    
    public PaymentOptions PaymentOptions { get; set; }

    public decimal? DiscountPrice { get; set; }

    public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice.Value < Price;

    public decimal FinalPrice => HasDiscount ? DiscountPrice!.Value : Price;

    public decimal? DiscountPercentage => HasDiscount
        ? Math.Round(100 * (Price - DiscountPrice!.Value) / Price, 2)
        : null;

    public string SellerId { get; set; }

    public QuantityStatus QuantityStatus { get; set; }

    public int Quantity { get; set; }

    public ProductDeliveryType DeliveryType { get; set; }
}