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
    public ObjectId Id { get; set; }

    /// <summary>
    /// Name of the category.
    /// Must be between 3 and 100 characters.
    /// </summary>
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

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
    public Category(string name, string? parentId = null)
    {
        Name = name;
        ParentId = parentId;
    }
}