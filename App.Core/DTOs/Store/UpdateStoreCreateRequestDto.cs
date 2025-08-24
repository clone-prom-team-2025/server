using App.Core.Enums;

namespace App.Core.DTOs.Store;

public class UpdateStoreCreateRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public StorePlans Plan { get; set; } = StorePlans.None;
}