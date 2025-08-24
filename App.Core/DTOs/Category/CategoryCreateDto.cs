using System.ComponentModel.DataAnnotations;

namespace App.Core.DTOs.Categoty;

public class CategoryCreateDto
{
    [Required] public Dictionary<string, string> Name { get; set; }

    public string? ParentId { get; set; }
}