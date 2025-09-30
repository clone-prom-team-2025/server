using App.Core.Enums;
using App.Core.Models.Product;
using App.Core.Models.Sell;

namespace App.Core.Archive.Product.DTOs;

public class ProductArchiveDto
{
    public string Id { get; set; } = string.Empty;

    public DateTime ArchivedAt { get; set; }

    public string ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<string> CategoryPath { get; set; } = [];

    public List<ProductFeature> Features { get; set; } = [];

    public decimal Price { get; set; }

    public PriceType PriceType { get; set; }

    public PaymentOptions PaymentOptions { get; set; }

    public decimal? DiscountPrice { get; set; }

    public string SellerId { get; set; }

    public QuantityStatus QuantityStatus { get; set; }

    public int Quantity { get; set; }

    public ProductDeliveryType DeliveryType { get; set; }

    public ProductDimensions ProductDimensions { get; set; } = new();
}