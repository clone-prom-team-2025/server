namespace App.Core.DTOs.Cart;

public class CreateCartDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Pcs { get; set; } = 0;
}