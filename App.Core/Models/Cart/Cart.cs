using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Cart;

public class Cart
{
    [BsonId] public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    public ObjectId UserId { get; set; }
    public int Pcs { get; set; } = 0;
    public ObjectId ProductId { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }
}