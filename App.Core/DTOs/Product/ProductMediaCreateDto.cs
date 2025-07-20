using App.Core.Enums;

namespace App.Core.DTOs.Product;

public class ProductMediaCreateDto
{
    public ProductMediaCreateDto(string productId, string url, MediaType type, int order)
    {
        ProductId = productId;
        Url = url;
        Type = type;
        Order = order;
    }

    public string ProductId { get; set; }

    public string Url { get; set; }

    public MediaType Type { get; set; }

    public int Order { get; set; }
}