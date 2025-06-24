namespace App.Core.Models.Product;

public class ProductFeatureItem
{
    public object value { get; set; }
    public string type { get; set; }
    public Dictionary<string, string> rules { get; set; } = new();
}