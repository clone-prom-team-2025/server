using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace App.Core.Models;

/// <summary>
/// Represents a product category in the system.
/// Categories can be organized hierarchically using the ParentId property.
/// </summary>
public class Category
{
    /// <summary>
    /// Unique identifier for the category (MongoDB ObjectId).
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <summary>
    /// Dictionary of localized names (e.g., { "ua": "Телефони", "en": "Phones" }).
    /// </summary>
    [Required]
    public Dictionary<string, string> Name { get; set; } = new();

    /// <summary>
    /// Optional reference to the parent category's ObjectId.
    /// Allows nesting categories hierarchically.
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentId { get; set; }

    /// <summary>
    ///  Empty constructor
    /// </summary>
    public Category() {}

    /// <summary>
    /// Constructor to create a category with initial values.
    /// </summary>
    /// <param name="name">Category name.</param>
    /// <param name="parentId">Id of the senior (parental) category.</param>
    public Category(Dictionary<string, string> name, string? parentId = null)
    {
        Name = new Dictionary<string, string>(name);
        ParentId = parentId;
    }
}