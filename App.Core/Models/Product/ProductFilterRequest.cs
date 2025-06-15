namespace App.Core.Models.Product;

/// <summary>
/// Request model to filter products by features and other criteria.
/// Includes pagination and inclusion/exclusion filters.
/// </summary>
public class ProductFilterRequest
{
    /// <summary>
    /// Product type to filter (e.g., "phone", "tv").
    /// </summary>
    public string ProductType { get; set; }

    /// <summary>
    /// Dictionary of feature keys and values to include in filtering.
    /// </summary>
    public Dictionary<string, object> Include { get; set; } = new();

    /// <summary>
    /// Dictionary of feature keys and values to exclude from results.
    /// </summary>
    public Dictionary<string, object> Exclude { get; set; } = new();

    /// <summary>
    /// Page number for pagination (starts from 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page for pagination.
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProductFilterRequest() {}

    /// <summary>
    /// Parameterized constructor for filtering request.
    /// </summary>
    /// <param name="productType">Product type to filter.</param>
    /// <param name="include">Features to include.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="exclude">Optional features to exclude.</param>
    public ProductFilterRequest(string productType, Dictionary<string, object> include, int page, int pageSize, Dictionary<string, object>? exclude = null)
    {
        ProductType = productType;
        Include = include;
        Page = page;
        PageSize = pageSize;
        Exclude = exclude ?? new Dictionary<string, object>();
    }
}