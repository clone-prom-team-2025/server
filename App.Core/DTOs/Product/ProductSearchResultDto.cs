namespace App.Core.DTOs.Product;

public class ProductSearchResultDto
{
    public string ProductId { get; set; } = string.Empty;

    public string Highlighted { get; set; } = string.Empty;

    public int Rank { get; set; }
}