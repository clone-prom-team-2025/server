using MongoDB.Bson;

namespace App.Core.Models.AvailableFilters;

/// <summary>
/// Represents a set of available filters for a specific product category.
/// </summary>
public class AvailableFilters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AvailableFilters"/> class with a specified ID, category ID, and list of filters.
    /// </summary>
    /// <param name="id">The unique identifier for the available filters.</param>
    /// <param name="categoryId">The ID of the associated category.</param>
    /// <param name="filters">The list of filter items available for the category.</param>
    public AvailableFilters(ObjectId id, ObjectId categoryId, List<AvailableFiltersItem> filters)
    {
        Id = id;
        CategoryId = categoryId;
        Filters = filters;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AvailableFilters"/> class with a category ID and list of filters. A new ID will be generated automatically.
    /// </summary>
    /// <param name="categoryId">The ID of the associated category.</param>
    /// <param name="filters">The list of filter items available for the category.</param>
    public AvailableFilters(ObjectId categoryId, List<AvailableFiltersItem> filters)
    {
        CategoryId = categoryId;
        Filters = filters;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the available filters.
    /// </summary>
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    /// <summary>
    /// Gets or sets the ID of the associated category.
    /// </summary>
    public ObjectId CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the list of available filter items.
    /// </summary>
    public List<AvailableFiltersItem> Filters { get; set; }
}