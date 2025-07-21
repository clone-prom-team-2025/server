namespace App.Core.DTOs.Product;

public class ProductFeatureDto
{
    public object Value { get; set; }

    public string Type { get; set; }

    public Dictionary<string, string> Rules { get; set; } = new();
}