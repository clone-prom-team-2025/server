using MongoDB.Bson;

namespace App.Core.Models.Product;

/// <summary>
///     Represents the result of a product search.
/// </summary>
public class ProductSearchResult
{
    /// <summary>
    ///     Product identifier.
    /// </summary>
    public ObjectId ProductId { get; set; } = ObjectId.GenerateNewId();

    /// <summary>
    ///     Highlighted string containing the search match.
    /// </summary>
    public string Highlighted { get; set; } = string.Empty;

    /// <summary>
    ///     Position index of the matched term.
    /// </summary>
    public int Rank { get; set; }
}