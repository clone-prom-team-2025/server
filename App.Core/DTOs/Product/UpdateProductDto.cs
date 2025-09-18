using App.Core.Enums;
using App.Core.Models.Sell;

namespace App.Core.DTOs.Product;

public class UpdateProductDto
{
    public UpdateProductDto(string id, string name, string category, List<ProductFeatureDto> features,
        string sellerId, int quantity, decimal price, PriceType priceType,
        PaymentOptions paymentOptions, ProductDeliveryType deliveryType, decimal? discountPrice = null)
    {
        Id = id;
        Name = name;
        Category = category;
        Features = [..features];
        SellerId = sellerId;
        Price = price;
        DiscountPrice = discountPrice;
        Quantity = quantity;
        PriceType = priceType;
        PaymentOptions = paymentOptions;
        DeliveryType = deliveryType;
    }

    public UpdateProductDto()
    {
    }

    public string Id { get; set; }

    public string Name { get; set; }

    public string Category { get; set; }

    public List<ProductFeatureDto> Features { get; set; }

    public decimal Price { get; set; }

    public PriceType PriceType { get; set; }

    public PaymentOptions PaymentOptions { get; set; }

    public decimal? DiscountPrice { get; set; }

    public string SellerId { get; set; }
    
    public int Quantity { get; set; }

    public ProductDeliveryType DeliveryType { get; set; }
    
    public ProductDimensions ProductDimensions { get; set; } = new ProductDimensions();
}