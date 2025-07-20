using System.ComponentModel.DataAnnotations;
using App.Core.Models.Product;
using MongoDB.Bson;

namespace App.Core.DTOs.Product;

public class ProductCreateDto
{
    public ProductCreateDto(Dictionary<string, string> name, string productType, List<string> categoryPath, List<ProductVariationDto> variations, string sellerId)
    {
        Name = new Dictionary<string, string>(name);
        ProductType = productType;
        CategoryPath = new List<string>(categoryPath);
        Variations = new List<ProductVariationDto>(variations);
        SellerId = sellerId;
    }

    [Required]
    public Dictionary<string, string> Name { get; set; }

    [StringLength(50)]
    public string ProductType { get; set; }

    public List<string> CategoryPath { get; set; }

    public List<ProductVariationDto> Variations { get; set; }

    public string SellerId { get; set; }
}