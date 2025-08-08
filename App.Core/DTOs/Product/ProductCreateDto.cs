using System.ComponentModel.DataAnnotations;
using App.Core.Models.Product;
using MongoDB.Bson;

namespace App.Core.DTOs.Product;

public class ProductCreateDto
{
    public ProductCreateDto(string name, string productType, string category, List<ProductFeatureDto> features, string sellerId, int quantity, string quantityStatus, decimal price, decimal? discountPrice = null)
    {
        Name = name;
        ProductType = productType;
        Category = category;
        Features = [..features];
        SellerId = sellerId;
        Price = price;
        DiscountPrice = discountPrice;
        QuantityStatus = quantityStatus;
        Quantity = quantity;
    }
    
    public string Name { get; set; }
    
    public string ProductType { get; set; }

    public string Category { get; set; }

    public List<ProductFeatureDto> Features { get; set; }
    
    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }
    
    public string SellerId { get; set; }
    
    public string QuantityStatus { get; set; }
    
    public int Quantity { get; set; }
}