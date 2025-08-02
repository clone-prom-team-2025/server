namespace App.Core.Models.AvailableFilters;

/// <summary>
/// Represents a single filter item used in the product filtering system.
/// </summary>
/// <param name="value">The value of the filter (e.g., "Red", "XL", "Plastic").</param>
/// <param name="type">The type of filter (e.g., "color", "size", "material").</param>
public class AvailableFiltersItem(string value, string type)
{
    /// <summary>
    /// Gets or sets the value of the filter.
    /// </summary>
    public string Value { get; set; } = value;

    /// <summary>
    /// Gets or sets the type of the filter.
    /// </summary>
    public string Type { get; set; } = type;
}