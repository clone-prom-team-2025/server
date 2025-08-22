using App.Core.Models.AvailableFilters;
using MongoDB.Bson;

namespace App.Core.DTOs.AvailableFilters;

public class AvailableFiltersCreateDto
{
    public AvailableFiltersCreateDto(string categoryId, List<AvailableFiltersItemDto> filters)
    {
        CategoryId = categoryId;
        Filters = filters;
    }

    public AvailableFiltersCreateDto()
    {
        
    }

    public string CategoryId { get; set; }

    public List<AvailableFiltersItemDto> Filters { get; set; }
}