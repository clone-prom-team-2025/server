namespace App.Core.Models.Product;

public class ProductFeature
{
    public string Category { get; set; }
    Dictionary<string, ProductFeatureItem> Features { get; set; } = new();
}