namespace App.Core.DTOs.Favorite;

public class FavoriteSellerDto
{
    public FavoriteSellerDto() { }

    public FavoriteSellerDto(string id, string userId, string name, List<string> sellers)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Sellers = sellers;
    }
    
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<string> Sellers { get; set; } = [];
}