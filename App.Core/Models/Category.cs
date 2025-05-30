using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace App.Core.Models;

public class Category
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentId { get; set; }
}