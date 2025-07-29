using App.Core.Enums;

namespace App.Core.DTOs.Product;

public class ProductMediaDto
{
    public ProductMediaDto(string id, string productId, string urlFileName, string url, MediaType type, int order, string? secondaryUrl = null, string? secondUrlFileName = null)
    {
        Id = id;
        ProductId = productId;
        UrlFileName = urlFileName;
        SecondUrlFileName = secondUrlFileName;
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

    public string UrlFileName { get; set; }

    public string? SecondUrlFileName { get; set; }

    public string Url { get; set; }

    public string? SecondaryUrl { get; set; }

    public MediaType Type { get; set; }

    public int Order { get; set; }
}