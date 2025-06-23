using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyApp.Core.Utils;

namespace App.Core.Models.Product;

/// <summary>
/// Represents a product stored in the MongoDB database.
/// Contains general product details, localization, categorization, and variations.
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for the product (MongoDB ObjectId as string).
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    /// <summary>
    /// Unique identifier of the product variation (automatically generated).
    /// </summary>
    [Required]
    public string UniqueId { get; set; }

    /// <summary>
    /// Dictionary of localized product names.
    /// Example: { "ua": "Кріплення універсальне", "en": "Universal fastening" }.
    /// </summary>
    [Required]
    public Dictionary<string, string> Name { get; set; }

    /// <summary>
    /// Product type or high-level category (e.g., "phone", "tv").
    /// </summary>
    [StringLength(50)]
    public string ProductType { get; set; }

    /// <summary>
    /// List of category hierarchy or path (e.g., ["Electronics", "Phones", "Smartphones"]).
    /// </summary>
    public List<string> CategoryPath { get; set; }

    /// <summary>
    /// List of all variations for this product (e.g., different colors, configurations).
    /// </summary>
    public List<ProductVariation> Variations { get; set; }
    
    /// <summary>
    /// Identifier of the seller (linked from PostgreSQL).
    /// </summary>
    public string SellerId { get; set; }

    /// <summary>
    /// Default constructor. Initializes the ID and lists.
    /// </summary>
    public Product()
    {
        Id = ObjectId.GenerateNewId().ToString();
        CategoryPath = [];
        Variations = [];
        Name = [];
    }

    /// <summary>
    /// Constructs a new Product instance with provided values.
    /// </summary>
    /// <param name="name">Localized names of the product.</param>
    /// <param name="productType">The product type/category.</param>
    /// <param name="categoryPath">The product's category hierarchy.</param>
    /// <param name="variations">The list of product variations.</param>
    /// <param name="sellerId">Reference to seller by his id</param>
    /// <exception cref="ArgumentNullException">Thrown when required arguments are null.</exception>
    public Product(
        Dictionary<string, string> name,
        string productType,
        List<string> categoryPath,
        List<ProductVariation> variations,
        string sellerId)
    {
        UniqueId = NanoIdGenerator.Generate(15);
        Id = ObjectId.GenerateNewId().ToString();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ProductType = productType;
        CategoryPath = categoryPath ?? throw new ArgumentNullException(nameof(categoryPath));
        Variations = variations ?? throw new ArgumentNullException(nameof(variations));
        SellerId = sellerId ?? throw new ArgumentNullException(nameof(sellerId));
    }
}
