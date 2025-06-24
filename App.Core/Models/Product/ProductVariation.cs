using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Product;

/// <summary>
/// Represents a product variation with media, price, description, and dynamic features.
/// </summary>
public class ProductVariation
{
    /// <summary>
    /// Optional model name of the product variation.
    /// </summary>
    [StringLength(100)]
    public string? ModelName { get; set; }

    /// <summary>
    /// Optional description of the product variation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// List of media items (images, videos) associated with the product.
    /// </summary>
    public List<ProductMedia> Media { get; set; } = [];
    
    /// <summary>
    /// Quantity available in stock.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Price of the product.
    /// </summary>
    public double Price { get; set; }

    /// <summary>
    /// Optional status of the quantity (e.g., "In stock", "Limited").
    /// </summary>
    [StringLength(40)]
    public string? QuantityStatus { get; set; }

    /// <summary>
    /// Dictionary of custom features (e.g., color, size).
    /// Stored as extra elements in MongoDB.
    /// </summary>
    [BsonExtraElements]
    public List<ProductFeature> Features { get; set; } = [];

    /// <summary>
    /// Full constructor for initializing all properties of a product variation.
    /// </summary>
    /// <param name="media">Media items (images/videos) related to the product.</param>
    /// <param name="features">Custom product features.</param>
    /// <param name="quantity">Amount of product available.</param>
    /// <param name="price">Product price.</param>
    /// <param name="modelName">Optional model name.</param>
    /// <param name="description">Optional description of the product.</param>
    /// <param name="quantityStatus">Optional quantity status ("In stock", etc.).</param>
    public ProductVariation(
        List<ProductMedia>? media,
        List<ProductFeature>? features,
        int quantity,
        double price,
        string? modelName = null,
        string? description = null,
        string? quantityStatus = null)
    {
        Media = media ?? [];
        Features = features ?? [];
        Quantity = quantity;
        Price = price;
        ModelName = modelName;
        Description = description;
        QuantityStatus = quantityStatus;
    }

    /// <summary>
    /// Default constructor that generates a new unique identifier.
    /// </summary>
    public ProductVariation()
    {
    }
}
