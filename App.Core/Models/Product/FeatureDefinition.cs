using System.ComponentModel.DataAnnotations;

namespace App.Core.Models.Product;

/// <summary>
/// Defines a feature key and its metadata used in product schemas.
/// Used to describe what features exist for a product type, their types, and display info.
/// </summary>
public class FeatureDefinition
{
    /// <summary>
    /// Unique key identifying the feature (e.g., "batteryCapacity").
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Key { get; set; }

    /// <summary>
    /// Human-readable label for the feature (e.g., "Battery Capacity").
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Label { get; set; }

    /// <summary>
    /// Data type of the feature (e.g., "number", "bool", "string").
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Type { get; set; }

    /// <summary>
    /// Optional unit for the feature (e.g., "mAh", "inch").
    /// </summary>
    [StringLength(20)]
    public string Unit { get; set; }

    /// <summary>
    /// Indicates if the feature can be used as a filter in queries.
    /// </summary>
    public bool Filterable { get; set; }
}