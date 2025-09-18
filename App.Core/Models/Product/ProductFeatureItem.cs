namespace App.Core.Models.Product;

/// <summary>
///     Represents a specific product feature value.
/// </summary>
public class ProductFeatureItem
{
    /// <summary>
    ///     Creates a new feature item with a value and type.
    /// </summary>
    /// <param name="value">The value of the feature.</param>
    /// <param name="type">The type of the feature (e.g., number, string).</param>
    public ProductFeatureItem(string value, string type, bool nullable = false)
    {
        Value = value;
        Type = type;
        Nullable = nullable;
    }

    /// <summary>
    ///     The value of the feature.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///     The type of the feature (e.g., string, number).
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     Optional validation rules for the feature.
    /// </summary>
    public bool Nullable { get; set; }
}