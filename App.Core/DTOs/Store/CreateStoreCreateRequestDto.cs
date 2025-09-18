using App.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace App.Core.DTOs.Store;

public class CreateStoreCreateRequestDto
{
    public string Name { get; set; } = string.Empty;
    public StorePlans Plan { get; set; } = StorePlans.None;
    public IFormFile File { get; set; }
}