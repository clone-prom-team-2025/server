namespace App.Core.Models;

/// <summary>
/// Represents a node in a hierarchical category tree structure.
/// Each node contains its own ID, localized name, and optional child categories.
/// </summary>
public class CategoryNode
{
    /// <summary>
    /// Unique identifier of the category (string representation of MongoDB ObjectId).
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Dictionary containing localized category names.
    /// Example: { "en": "Phones", "ua": "Телефони" }.
    /// </summary>
    public Dictionary<string, string> Name { get; set; } = new();

    /// <summary>
    /// List of child category nodes (subcategories).
    /// </summary>
    public List<CategoryNode> Children { get; set; } = new();
}