namespace App.Core.DTOs.Product;

public class ProductFeatureDto
{
    public ProductFeatureDto(string category)
    {
        Category = category;
    }

    public string Category { get; set; }

    public Dictionary<string, ProductFeatureItemDto> Features { get; set; } = new();
}