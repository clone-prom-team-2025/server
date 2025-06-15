using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Product;

/// <summary>
/// Schema definition for a product type.
/// Specifies the product type name and the list of feature definitions it contains.
/// </summary>
public class ProductSchema
{
    /// <summary>
    /// Unique identifier for the schema.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <summary>
    /// Product type this schema describes (e.g., "phone").
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Type { get; set; }

    /// <summary>
    /// Human-readable display name for the product type.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; }

    /// <summary>
    /// List of feature definitions describing the fields available for products of this type.
    /// </summary>
    [Required]
    public List<FeatureDefinition> Features { get; set; } = new();
}