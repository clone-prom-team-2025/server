namespace App.Core.DTOs.Favorite;

public class FavoriteProductDto
{
    public FavoriteProductDto()
    {
    }

    public FavoriteProductDto(string id, string userId, string name, List<string> products)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Products = products;
    }

    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<string> Products { get; set; } = [];
}