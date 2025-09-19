using App.Core.Enums;

namespace App.Core.DTOs.Product;

public class ProductFilterRequestDto
{
    public ProductFilterRequestDto()
    {
    }

    public ProductFilterRequestDto(string? categoryId, int page, int pageSize,
        Dictionary<string, List<string>>? include = null,
        Dictionary<string, List<string>>? exclude = null,
        SortDirection sort = SortDirection.None,
        decimal? priceMax = null,
        decimal? priceMin = null)
    {
        CategoryId = categoryId;
        Include = include ?? new Dictionary<string, List<string>>();
        Page = page;
        PageSize = pageSize;
        Exclude = exclude ?? new Dictionary<string, List<string>>();
        Sort = sort;
        PriceMax = priceMax;
        PriceMin = priceMin;
    }

    public string? CategoryId { get; set; }

    public Dictionary<string, List<string>> Include { get; set; } = new();

    public Dictionary<string, List<string>> Exclude { get; set; } = new();

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public decimal? PriceMin { get; set; }

    public decimal? PriceMax { get; set; }

    public SortDirection Sort { get; set; } = SortDirection.None;
}