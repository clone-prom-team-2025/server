using App.Core.Models.FileStorage;
using MongoDB.Bson;

namespace App.Core.Models.Sell;

public class MiniProductInfo
{
    public ObjectId ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public BaseFile Image { get; set; } = new();
    public decimal Price { get; set; }
}