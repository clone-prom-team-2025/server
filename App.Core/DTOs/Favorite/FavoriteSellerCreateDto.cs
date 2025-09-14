namespace App.Core.DTOs.Favorite;

public class FavoriteSellerCreateDto
{
    public FavoriteSellerCreateDto() { }

    public FavoriteSellerCreateDto(string userId, string name, List<string> seller)
    {
        UserId = userId;
        Name = name;
        Sellers = seller;
    }
    
    public string UserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<string> Sellers { get; set; } = [];
}