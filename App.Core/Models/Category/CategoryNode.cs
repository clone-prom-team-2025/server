namespace App.Core.Models;

public class CategoryNode
{
    public string Id { get; set; }
    public Dictionary<string, string> Name { get; set; } = new();
    public List<CategoryNode> Children { get; set; } = new();
}
