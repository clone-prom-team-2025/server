using System.ComponentModel.DataAnnotations;

namespace App.Core.DTOs.Product;

public class ProductDto
{
    public ProductDto(string id, Dictionary<string, string> name, string productType, List<string> categoryPath, List<ProductVariationDto> variations, string sellerId)
    {
        Id = id;
        Name = new Dictionary<string, string>(name);
        ProductType = productType;
        CategoryPath = new List<string>(categoryPath);
        Variations = new List<ProductVariationDto>(variations);
        SellerId = sellerId;
    }

    public ProductDto()
    {
        
    }

    public string Id { get; set; }

    [Required]
    public Dictionary<string, string> Name { get; set; }

    [StringLength(50)]
    public string ProductType { get; set; }

    public List<string> CategoryPath { get; set; }

    public List<ProductVariationDto> Variations { get; set; }

    public string SellerId { get; set; }
    
    public double MinPrice { get; set; }
    
    public double MaxPrice { get; set; }
}