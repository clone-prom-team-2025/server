namespace App.Core.DTOs.AvailableFilters;

public class AvailableFiltersDto
{
    public AvailableFiltersDto(string id, string categoryId, List<AvailableFiltersItemDto> filters)
    {
        Id = id;
        CategoryId = categoryId;
        Filters = filters;
    }

    public string Id { get; set; }

    public string CategoryId { get; set; }

    public List<AvailableFiltersItemDto> Filters { get; set; }
}