using System.ComponentModel.DataAnnotations;

namespace App.Core.DTOs;

public class CategoryCreateDto
{
    [Required]
    public Dictionary<string, string> Name { get; set; }
    public string? ParentId { get; set; }
}