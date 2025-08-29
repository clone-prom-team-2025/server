namespace App.Core.DTOs.Product;

public class ProductFilterResponseDto
{
    public decimal PriceFrom { get; set; }
    public decimal PriceTo { get; set; }
    public int Pages { get; set; }
    public int Count { get; set; }
    public List<ProductDto> Products { get; set; } = [];
}