namespace App.Core.DTOs.AvailableFilters;

public class AvailableFiltersItemDto(string title, List<string> values)
{

    public string Title { get; set; } = title;
    
    public List<string> Values { get; set; } = values;
}