namespace App.Core.Models.Product;

public class ProductFilterResponse
{
    public decimal PriceFrom { get; set; }
    public decimal PriceTo { get; set; }
    public int Pages { get; set; }
    public int Count { get; set; }
    public List<Product> Products { get; set; } = [];
}