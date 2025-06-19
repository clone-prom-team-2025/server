using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace App.Core.Models;

/// <summary>
/// Represents a product category in the system.
/// Categories support hierarchical organization using the <see cref="ParentId"/> field,
/// and localized names through the <see cref="Name"/> dictionary.
/// </summary>
public class Category
{
    /// <summary>
    /// Unique identifier for the category (MongoDB ObjectId as a string).
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    /// <summary>
    /// Dictionary containing localized names for the category.
    /// Example: { "ua": "Телефони", "en": "Phones" }.
    /// </summary>
    [Required]
    public Dictionary<string, string> Name { get; set; } = new();

    /// <summary>
    /// Optional reference to the parent category's ID.
    /// If null, this category is considered a root-level category.
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Category"/> class.
    /// </summary>
    public Category() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="Category"/> class with specified values.
    /// </summary>
    /// <param name="name">Dictionary of localized names.</param>
    /// <param name="parentId">ID of the parent category (optional).</param>
    public Category(Dictionary<string, string> name, string? parentId = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ParentId = parentId;
    }
}