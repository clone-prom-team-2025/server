using System.ComponentModel.DataAnnotations;

namespace App.Core.DTOs.Product;

public class ProductDto
{
    public ProductDto(string id, string name, string productType, List<string> categoryPath, List<ProductFeatureDto> features, string sellerId, int quantity, string quantityStatus, decimal price, decimal? discountPrice = null)
    {
        Id = id;
        Name = name;
        ProductType = productType;
        CategoryPath = new List<string>(categoryPath);
        Features = [..features];
        SellerId = sellerId;
        Price = price;
        DiscountPrice = discountPrice;
        QuantityStatus = quantityStatus;
        Quantity = quantity;
    }

    public ProductDto()
    {
        
    }

    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public string ProductType { get; set; }

    public List<string> CategoryPath { get; set; }

    public List<ProductFeatureDto> Features { get; set; }
    
    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }
    
    public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice.Value < Price;

    public decimal FinalPrice => HasDiscount ? DiscountPrice!.Value : Price;

    public decimal? DiscountPercentage => HasDiscount
        ? Math.Round(100 * (Price - DiscountPrice!.Value) / Price, 2)
        : null;

    public string SellerId { get; set; }
    
    public string QuantityStatus { get; set; }
    
    public int Quantity { get; set; }
}