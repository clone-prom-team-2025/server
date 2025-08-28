using App.Core.Enums;

namespace App.Core.DTOs.Product;

public class ProductCreateDto
{
    public ProductCreateDto(string name, string category, List<ProductFeatureDto> features,
        string sellerId, int quantity, QuantityStatus quantityStatus, decimal price, PriceType priceType, PaymentOptions paymentOptions, decimal? discountPrice = null)
    {
        Name = name;
        Category = category;
        Features = [..features];
        SellerId = sellerId;
        Price = price;
        DiscountPrice = discountPrice;
        QuantityStatus = quantityStatus;
        Quantity = quantity;
        PriceType = priceType;
        PaymentOptions = paymentOptions;
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

    public string SellerId { get; set; }

    public QuantityStatus QuantityStatus { get; set; }

    public int Quantity { get; set; }
}