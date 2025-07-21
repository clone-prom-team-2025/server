using App.Core.Enums;

namespace App.Core.DTOs.Product;

public class ProductMediaDto
{
    public ProductMediaDto(string id, string productId, string url, MediaType type, int order)
    {
        Id = id;
        ProductId = productId;
        Url = url;
        Type = type;
        Order = order;
    }

    public string Id { get; set; }

    public string ProductId { get; set; }

    public string Url { get; set; }

    public MediaType Type { get; set; }

    public int Order { get; set; }
}