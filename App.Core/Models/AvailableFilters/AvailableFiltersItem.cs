namespace App.Core.Models.AvailableFilters;

/// <summary>
///     Represents a single filter item used in the product filtering system.
/// </summary>
/// <param name="title">The title of the filter (e.g., "Manufacturer", "Ingredients").</param>
/// <param name="values">The values of filter (e.g., "OnePlus", "Google", "Samsung").</param>
public class AvailableFiltersItem(string title, List<string> values)
{
    /// <summary>
    ///     Gets or sets the title of the filter.
    /// </summary>
    public string Title { get; set; } = title;

    /// <summary>
    ///     Gets or sets the values of the filter.
    /// </summary>
    public List<string> Values { get; set; } = values;
}