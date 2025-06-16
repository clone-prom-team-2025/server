using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyApp.Core.Utils;
using NanoidDotNet;

namespace App.Core.Models.Product;

/// <summary>
/// Represents a product stored in the MongoDB database.
/// Contains general product details and a flexible dictionary of features.
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for the product (MongoDB ObjectId).
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public string UniqueId { get; set; }
    
    /// <summary>
    /// Name of the product.
    /// </summary>
    [StringLength(200)]
    public string Name { get; set; }
    
    /// <summary>
    /// Type/category of the product (e.g., "phone", "tv").
    /// </summary>
    [StringLength(50)]
    public string ProductType { get; set; }
    
    /// <summary>
    /// Optional description of the product.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// List of media items (images, videos) related to the product.
    /// </summary>
    private List<ProductMedia> Media { get; set; }

    /// <summary>
    /// List representing the category path or hierarchy for the product.
    /// </summary>
    private List<string> CategoryPath { get; set; }
    
    /// <summary>
    /// Flexible dictionary of product features and their values.
    /// Keys correspond to feature keys defined in the product schema.
    /// </summary>
    [BsonExtraElements]
    public Dictionary<string, object> Features { get; set; }
    
    /// <summary>
    /// Identifier of the seller (linked from PostgreSQL).
    /// </summary>
    public int SellerId { get; set; }
    
    /// <summary>
    /// Quantity available in stock.
    /// </summary>
    private int Quantity { get; set; }
    
    /// <summary>
    /// Optional status of the quantity (e.g., "In stock", "Limited").
    /// </summary>
    [StringLength(40)]
    public string? QuantityStatus { get; set; }
    
    /// <summary>
    /// Price of the product.
    /// </summary>
    double Price { get; set; }
    
    /// <summary>
    /// Default constructor initializes IDs and collections.
    /// </summary>
    public Product()
    {
        Id = ObjectId.GenerateNewId().ToString();
        Features = new Dictionary<string, object>();
        Media = new List<ProductMedia>();
        CategoryPath = new List<string>();
        UniqueId = NanoIdGenerator.Generate(15);
    }

    /// <summary>
    /// Constructor to create a product with initial values.
    /// </summary>
    /// <param name="name">Product name.</param>
    /// <param name="productType">Type or category of the product.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="categoryPath">List of categories/hierarchy.</param>
    /// <param name="features">Dictionary of product features and values.</param>
    /// <param name="sellerid">Seller identifier.</param>
    /// <param name="quantity">Available quantity.</param>
    /// <param name="quantityStatus">Optional quantity status.</param>
    /// <param name="price">Price of the product.</param>
    public Product(string name, string productType, string? description, List<string> categoryPath, Dictionary<string, object>? features, int sellerid, int quantity, string? quantityStatus, double price)
    {
        Id = ObjectId.GenerateNewId().ToString();
        Name = name;
        ProductType = productType;
        Description = description ?? null;
        Media = new List<ProductMedia>();
        CategoryPath = new List<string>(categoryPath);
        Features = features ?? new Dictionary<string, object>();
        SellerId = sellerid;
        Quantity = quantity;
        QuantityStatus = quantityStatus ?? null;
        Price = price;
        UniqueId = NanoIdGenerator.Generate(25);
    }
}