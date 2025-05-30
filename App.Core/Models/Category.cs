using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models;

public class Category
{
    [BsonId]
    public ObjectId Id { get; set; }
}