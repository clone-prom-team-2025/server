using App.Core.Enums;
using App.Core.Models.Product;
using App.Core.Models.Sell;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Archive.Product;

public class ProductArchive
{
    [BsonId]
    public string Id { get; set; } = string.Empty;
    
    public DateTime ArchivedAt { get; set; }
    
    public ObjectId ProductId { get; set; } = ObjectId.GenerateNewId();
    
    public string Name { get; set; } = string.Empty;
    
    public List<ObjectId> CategoryPath { get; set; } = [];
    
    public List<ProductFeature> Features { get; set; } = [];

    public decimal Price { get; set; }

    public PriceType PriceType { get; set; }

    public PaymentOptions PaymentOptions { get; set; }

    public decimal? DiscountPrice { get; set; }

    public ObjectId SellerId { get; set; } = ObjectId.Empty;

    public QuantityStatus QuantityStatus { get; set; }

    public int Quantity { get; set; }

    public ProductDeliveryType DeliveryType { get; set; }
    
    public ProductDimensions ProductDimensions { get; set; } = new ProductDimensions();
}