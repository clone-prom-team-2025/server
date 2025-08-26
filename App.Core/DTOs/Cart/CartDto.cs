namespace App.Core.DTOs.Cart;

public class CartDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int Pcs { get; set; } = 0;
    public string ProductId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
}