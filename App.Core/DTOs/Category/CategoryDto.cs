namespace App.Core.DTOs.Categoty;

public class CategoryDto
{
    public CategoryDto(string id, string name, string? parentId)
    {
        Id = id;
        Name = name;
        ParentId = parentId;
    }

    public string Id { get; set; }

    public string Name { get; set; }

    public string? ParentId { get; set; }
}