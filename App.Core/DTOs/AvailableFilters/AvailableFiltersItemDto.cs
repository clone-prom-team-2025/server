namespace App.Core.DTOs.AvailableFilters;

public class AvailableFiltersItemDto(string value, string type)
{

    public string Value { get; set; } = value;
    
    public string Type { get; set; } = type;
}