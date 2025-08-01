namespace App.Core.DTOs.Categoty;

public class CategoryDto
{
    public CategoryDto(string id, IDictionary<string, string> name, string? parentId)
    {
        Id = id;
        Name = new Dictionary<string, string>(name);
        ParentId = parentId;
    }

    public string Id { get; set; }

    public Dictionary<string, string> Name { get; set; } = new();

    public string? ParentId { get; set; }
}