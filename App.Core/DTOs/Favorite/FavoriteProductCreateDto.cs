namespace App.Core.DTOs.Favorite;

public class FavoriteProductCreateDto
{
    public FavoriteProductCreateDto() { }

    public FavoriteProductCreateDto(string userId, string name, List<string> products)
    {
        UserId = userId;
        Name = name;
        Products = products;
    }
    
    public string UserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<string> Products { get; set; } = [];
}