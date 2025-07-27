using App.Core.Enums;

namespace App.Core.DTOs.Product;

public class ProductMediaDto
{
    public ProductMediaDto(string id, string productId, string fileName, string url, MediaType type, int order, string? secondaryUrl = null)
    {
        Id = id;
        ProductId = productId;
        FileName = fileName;
        Url = url;
        Type = type;
        Order = order;
        SecondaryUrl = secondaryUrl;
    }

    public ProductMediaDto()
    {
        
    }

    public string Id { get; set; }

    public string ProductId { get; set; }

    public string FileName { get; set; }

    public string Url { get; set; }

    public string? SecondaryUrl { get; set; }

    public MediaType Type { get; set; }

    public int Order { get; set; }
}