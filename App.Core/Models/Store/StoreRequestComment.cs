using MongoDB.Bson;

namespace App.Core.Models.Store;

public record StoreRequestComment(ObjectId UserId, string Text);