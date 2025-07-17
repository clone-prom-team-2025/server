using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Product;

/// <summary>
///     Represents a product stored in the MongoDB database.
///     Contains general product details, localization, categorization, and variations.
/// </summary>/// <summary>
///     Represents a product stored in MongoDB.
///     Contains localization, categorization, and variation data.
/// </summary>
public class Product
{
    /// <summary>
    ///     Default constructor initializing ID and collections.
    /// </summary>
    public Product()
    {
        Id = ObjectId.GenerateNewId().ToString();
        CategoryPath = [];
        Variations = [];
        Name = [];
    }

    public Product(Product product)
    {
        Id = product.Id;
        Name = new Dictionary<string, string>(product.Name);
        ProductType = product.ProductType;
        CategoryPath = new List<string>(product.CategoryPath);
        Variations = new List<ProductVariation>(product.Variations);
        SellerId = product.SellerId;
        
    }

    /// <summary>
    ///     Constructs a new Product instance with provided values.
    /// </summary>
    /// <param name="name">Localized product names.</param>
    /// <param name="productType">Product type or classification.</param>
    /// <param name="categoryPath">Hierarchy of categories.</param>
    /// <param name="variations">List of product variations.</param>
    /// <param name="sellerId">Seller's identifier.</param>
    public Product(
        Dictionary<string, string> name,
        string productType,
        List<string> categoryPath,
        List<ProductVariation> variations,
        string sellerId)
    {
        Id = ObjectId.GenerateNewId().ToString();
        Name = name;
        ProductType = productType;
        CategoryPath = categoryPath;
        Variations = variations;
        SellerId = sellerId;
    }

    /// <summary>
    ///     Unique product identifier (MongoDB ObjectId as string).
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <summary>
    ///     Localized product names (e.g., {"en": "Lamp", "ua": "Лампа"}).
    /// </summary>
    [Required]
    public Dictionary<string, string> Name { get; set; }

    /// <summary>
    ///     Product type or category.
    /// </summary>
    [StringLength(50)]
    public string ProductType { get; set; }

    /// <summary>
    ///     Product category hierarchy.
    /// </summary>
    public List<string> CategoryPath { get; set; }

    /// <summary>
    ///     List of product variations (e.g., different configurations).
    /// </summary>
    public List<ProductVariation> Variations { get; set; }

    /// <summary>
    ///     List of product images.
    /// </summary>
    public List<ProductMedia> Images { get; set; } = new();

    /// <summary>
    ///     Identifier of the associated seller.
    /// </summary>
    public string SellerId { get; set; }
}
