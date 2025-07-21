using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyApp.Core.Utils;

namespace App.Core.Models.Product;

/// <summary>
/// Represents a single variation of a product, including specific media, features,
/// pricing, stock quantity, and optional metadata such as model name or description.
/// </summary>
public class ProductVariation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductVariation"/> class with all properties.
    /// </summary>
    /// <param name="media">A list of media items (e.g., images) associated with this variation.</param>
    /// <param name="features">A list of custom features that define the variation.</param>
    /// <param name="quantity">The available stock quantity for this variation.</param>
    /// <param name="price">The price of this variation.</param>
    /// <param name="modelName">An optional model name for identification or display.</param>
    /// <param name="description">An optional textual description of this variation.</param>
    /// <param name="quantityStatus">An optional text label describing the stock status (e.g., "In Stock", "Out of Stock").</param>
    public ProductVariation(
        List<ObjectId>? media,
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
        ModelId = NanoIdGenerator.Generate(10);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductVariation"/> class with default values.
    /// </summary>
    public ProductVariation()
    {
        ModelId = NanoIdGenerator.Generate(10);
    }

    /// <summary>
    /// Gets or sets an optional model name for the variation.
    /// </summary>
    [StringLength(100)]
    public string? ModelName { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this variation model.
    /// </summary>
    public string ModelId { get; set; }

    /// <summary>
    /// Gets or sets an optional description for the variation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the list of media files associated with this variation (e.g., images, videos).
    /// </summary>
    public List<ObjectId> Media { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of items available in stock for this variation.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the price for this variation.
    /// </summary>
    public double Price { get; set; }

    /// <summary>
    /// Gets or sets an optional textual representation of stock status (e.g., "Low Stock", "Sold Out").
    /// </summary>
    [StringLength(40)]
    public string? QuantityStatus { get; set; }

    /// <summary>
    /// Gets or sets a list of additional product features specific to this variation.
    /// Any unknown fields will also be deserialized into this list.
    /// </summary>
    [BsonExtraElements]
    public List<ProductFeature> Features { get; set; } = [];
}
