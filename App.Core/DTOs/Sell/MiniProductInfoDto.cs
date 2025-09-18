using App.Core.Models.FileStorage;

namespace App.Core.DTOs.Sell;

public class MiniProductInfoDto
{
    public string ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public BaseFile Image { get; set; } = new();
    public decimal Price { get; set; }
}