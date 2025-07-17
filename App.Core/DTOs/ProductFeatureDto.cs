namespace App.Core.DTOs;

/// <summary>
///     Represents a product feature with a typed value, its type description, and validation rules.
/// </summary>
/// <typeparam name="T">The type of the feature's value.</typeparam>
public class ProductFeatureDto
{
    /// <summary>
    ///     The value of the feature.
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    ///     The type of the value as a string (e.g., "string", "number", "boolean").
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     A list of validation rules that apply to the feature's value.
    /// </summary>
    public Dictionary<string, string> Rules { get; set; } = new();
}

public class ProductCreateDto
{
    public Dictionary<string, string> Name { get; set; }
    public string ProductType { get; set; }
    public List<string> CategoryPath { get; set; }
    public string SellerId { get; set; }
    public List<ProductMedia> Images { get; set; }
}

public class ProductUpdateDto
{
    public Dictionary<string, string> Name { get; set; }
    public string ProductType { get; set; }
    public List<string> CategoryPath { get; set; }
    public List<ProductMedia> Images { get; set; }
}