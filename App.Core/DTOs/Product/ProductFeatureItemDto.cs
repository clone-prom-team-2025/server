namespace App.Core.DTOs.Product;

public class ProductFeatureItemDto
{
    public ProductFeatureItemDto()
    {
    }

    public ProductFeatureItemDto(string value, string type, bool nullable = false)
    {
        Value = value;
        Type = type;
        Nullable = nullable;
    }

    public string Value { get; set; }

    public string Type { get; set; }

    public bool Nullable { get; set; }
}