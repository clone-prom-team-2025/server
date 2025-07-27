using App.Core.Enums;

namespace App.Core.DTOs.Product;

public class ProductMediaCreateDto
{
    public ProductMediaCreateDto(string productId, string url, int order, string? id = null)
    {
        ProductId = productId;
        Url = url;
        Order = order;
        Id = id;
    }

    public ProductMediaCreateDto()
    {
        
    }

    public string? Id;

    public string ProductId { get; set; }

    public string Url { get; set; }

    public int Order { get; set; }
}