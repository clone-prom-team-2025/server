namespace App.Core.Models.Product;

/// <summary>
///     Represents a feature category with multiple feature items.
/// </summary>
public class ProductFeature
{
    /// <summary>
    ///     Creates a new ProductFeature with the specified category.
    /// </summary>
    /// <param name="category">Feature category name.</param>
    public ProductFeature(string category)
    {
        Category = category;
    }

    /// <summary>
    ///     The name of the feature category.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    ///     Dictionary of feature items within the category.
    /// </summary>
    public Dictionary<string, ProductFeatureItem> Features { get; set; } = new();
}