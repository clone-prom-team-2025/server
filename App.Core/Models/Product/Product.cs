using App.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Product;

/// <summary>
///     Represents a product stored in the MongoDB database.
///     Contains general product details, localization, categorization, and variations.
/// </summary>
public class Product
{
    /// <summary>
    ///     Empty constructor.
    /// </summary>
    public Product()
    {
    }

    public Product(Product product)
    {
        Id = product.Id;
        Name = product.Name;
        CategoryPath = new List<ObjectId>(product.CategoryPath);
        Features = new List<ProductFeature>(product.Features);
        SellerId = product.SellerId;
        Price = product.Price;
        DiscountPrice = product.DiscountPrice;
        Quantity = product.Quantity;
        QuantityStatus = product.QuantityStatus;
        PriceType = product.PriceType;
        PaymentOptions = product.PaymentOptions;
    }

    /// <summary>
    ///     Constructs a new Product instance with provided values.
    /// </summary>
    /// <param name="name">Product names.</param>
    /// <param name="productType">Product type or classification.</param>
    /// <param name="categoryPath">Hierarchy of categories.</param>
    /// <param name="features">List of product features.</param>
    /// <param name="sellerId">Seller's identifier.</param>
    /// <param name="price">Price of product</param>
    /// <param name="discountPrice">Discount of product</param>
    public Product(
        string name,
        List<ObjectId> categoryPath,
        List<ProductFeature> features,
        ObjectId sellerId,
        int quantity,
        QuantityStatus quantityStatus,
        decimal price,
        PriceType priceType,
        PaymentOptions paymentOptions,
        decimal? discountPrice = null)
    {
        Name = name;
        CategoryPath = categoryPath;
        Features = features;
        SellerId = sellerId;
        Quantity = quantity;
        QuantityStatus = quantityStatus;
        Price = price;
        DiscountPrice = discountPrice;
        PriceType = priceType;
        PaymentOptions = paymentOptions;
    }

    /// <summary>
    ///     Unique product identifier (MongoDB ObjectId as string).
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    /// <summary>
    ///     Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Product category hierarchy.
    /// </summary>
    public List<ObjectId> CategoryPath { get; set; } = [];

    /// <summary>
    ///     Gets or sets a list of additional product features specific to this product.
    ///     Any unknown fields will also be deserialized into this list.
    /// </summary>
    public List<ProductFeature> Features { get; set; } = [];

    /// <summary>
    ///     Price of product
    /// </summary>
    public decimal Price { get; set; }
    
    public PriceType PriceType { get; set; }
    
    public PaymentOptions PaymentOptions { get; set; }

    /// <summary>
    ///     Discount of product
    /// </summary>
    public decimal? DiscountPrice { get; set; }

    /// <summary>
    ///     Identifier of the associated seller.
    /// </summary>
    public ObjectId SellerId { get; set; } = ObjectId.Empty;

    public QuantityStatus QuantityStatus { get; set; }

    public int Quantity { get; set; }
}