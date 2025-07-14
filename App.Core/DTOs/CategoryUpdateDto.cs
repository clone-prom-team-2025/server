using System.ComponentModel.DataAnnotations;

namespace App.Core.DTOs;

public class CategoryUpdateDto
{
    [Required]
    public Dictionary<string, string> Name { get; set; }
    public string? ParentId { get; set; }
}