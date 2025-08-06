using App.Core.Enums;
using MongoDB.Bson;

namespace App.Core.Models.Product;

/// <summary>
///     Request model to filter products by features and other criteria.
///     Includes pagination and inclusion/exclusion filters.
/// </summary>/// <summary>
///     Request object for filtering products by criteria.
/// </summary>
public class ProductFilterRequest
{
    /// <summary>
    ///     Default constructor.
    /// </summary>
    public ProductFilterRequest() {}

    /// <summary>
    ///     Creates a new filtering request with the given parameters.
    /// </summary>
    /// <param name="categoryId">Category of products to filter.</param>
    /// <param name="page">Page number for pagination.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="include">Features to include in the filter.</param>
    /// <param name="exclude">Features to exclude from the filter.</param>
    /// <param name="language">Language code for localization.</param>
    /// <param name="sortDirection">Sort direction</param>
    public ProductFilterRequest(ObjectId? categoryId, int page, int pageSize,
        Dictionary<string, string>? include = null,
        Dictionary<string, string>? exclude = null,
        string? language = "en", SortDirection sortDirection = SortDirection.None)
    {
        CategoryId = categoryId;
        Include = include ?? new();
        Page = page;
        PageSize = pageSize;
        Exclude = exclude ?? new();
        Language = language ?? "en";
        Sort = sortDirection;
    }

    /// <summary>
    ///     Type of product to filter.
    /// </summary>
    public ObjectId? CategoryId { get; set; }

    /// <summary>
    ///     Features to include in filtering.
    /// </summary>
    public Dictionary<string, string> Include { get; set; } = new();

    /// <summary>
    ///     Features to exclude from filtering.
    /// </summary>
    public Dictionary<string, string> Exclude { get; set; } = new();

    /// <summary>
    ///     Current page number for pagination.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    ///     Number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    ///     Language code for localized results.
    /// </summary>
    public string Language { get; set; } = "en";
    
    /// <summary>
    /// Sort direction
    /// </summary>
    public SortDirection Sort { get; set; } = SortDirection.None;
}