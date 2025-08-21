using App.Core.Enums;
using App.Core.Models.FileStorage;

namespace App.Core.DTOs.Product;

public class ProductMediaDto
{
    public ProductMediaDto(string id, string productId, MediaType type, int order, BaseFile files)
    {
        Id = id;
        ProductId = productId;
        Type = type;
        Order = order;
        Files = files;
    }

    public ProductMediaDto()
    {
        
    }

    public string Id { get; set; }

    public string ProductId { get; set; }

    public BaseFile Files { get; set; }

    public MediaType Type { get; set; }

    public int Order { get; set; }
}