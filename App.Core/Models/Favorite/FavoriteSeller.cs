using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Favorite;

public class FavoriteSeller
{
    public FavoriteSeller()
    {
    }

    public FavoriteSeller(ObjectId userId, string name)
    {
        UserId = userId;
        Name = name;
    }

    [BsonId] public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public ObjectId UserId { get; set; } = ObjectId.Empty;
    public string Name { get; set; } = null!;
    public List<ObjectId> Sellers { get; set; } = [];
}