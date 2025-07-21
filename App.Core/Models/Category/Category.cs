using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models;

/// <summary>
///     Represents a product category in the system.
///     Categories support hierarchical organization using the <see cref="ParentId" /> field,
///     and localized names through the <see cref="Name" /> dictionary.
/// </summary>
public class Category
{

    public Category(ObjectId id, IDictionary<string, string> name, ObjectId? parentId)
    {
        Id = id;
        Name = new Dictionary<string, string>(name);
        ParentId = parentId;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Category" /> class with specified values.
    /// </summary>
    /// <param name="name">Dictionary of localized names.</param>
    /// <param name="parentId">ID of the parent category (optional).</param>
    public Category(Dictionary<string, string> name, ObjectId? parentId = null)
    {
        Id = ObjectId.GenerateNewId();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ParentId = parentId;
    }

    /// <summary>
    ///     Unique identifier for the category (MongoDB ObjectId as a string).
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; }

    /// <summary>
    ///     Dictionary containing localized names for the category.
    ///     Example: { "ua": "Телефони", "en": "Phones" }.
    /// </summary>
    [Required]
    public Dictionary<string, string> Name { get; set; } = new();

    /// <summary>
    ///     Optional reference to the parent category's ID.
    ///     If null, this category is considered a root-level category.
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId? ParentId { get; set; }
}