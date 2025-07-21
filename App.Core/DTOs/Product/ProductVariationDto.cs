using System.ComponentModel.DataAnnotations;
using App.Core.Models.Product;

namespace App.Core.DTOs.Product;

public class ProductVariationDto
{
    public ProductVariationDto(
        List<string>? media,
        List<ProductFeatureDto>? features,
        int quantity,
        double price,
        string modelId,
        string? modelName = null,
        string? description = null,
        string? quantityStatus = null)
    {
        Media = media ?? [];
        Features = features ?? [];
        Quantity = quantity;
        Price = price;
        ModelName = modelName;
        Description = description;
        QuantityStatus = quantityStatus;
        ModelId = modelId;
    }

    [StringLength(100)]
    public string? ModelName { get; set; }

    public string ModelId { get; set; }

    public string? Description { get; set; }

    public List<string> Media { get; set; } = [];

    public int Quantity { get; set; }

    public double Price { get; set; }

    [StringLength(40)]
    public string? QuantityStatus { get; set; }

    public List<ProductFeatureDto> Features { get; set; }
}