using App.Core.Enums;
using MongoDB.Bson;

namespace App.Core.DTOs.Product;

public class ProductFilterRequestDto
{
    public ProductFilterRequestDto() {}
    
    public ProductFilterRequestDto(string? categoryId, int page, int pageSize,
        Dictionary<string, string>? include = null,
        Dictionary<string, string>? exclude = null,
        string? language = "en", SortDirection sort = SortDirection.None)
    {
        CategoryId = categoryId;
        Include = include ?? new();
        Page = page;
        PageSize = pageSize;
        Exclude = exclude ?? new();
        Language = language ?? "en";
        Sort = sort;
    }
    
    public string? CategoryId { get; set; }
    
    public Dictionary<string, string> Include { get; set; } = new();
    
    public Dictionary<string, string> Exclude { get; set; } = new();
    
    public int Page { get; set; } = 1;
    
    public int PageSize { get; set; } = 20;
    
    public string Language { get; set; } = "en";
    
    public SortDirection Sort { get; set; } = SortDirection.None;
}