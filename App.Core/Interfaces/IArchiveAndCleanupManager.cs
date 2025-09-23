using App.Core.Archive.Product;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IArchiveAndCleanupManager
{
    Task SoftDeleteProductAsync(ObjectId id);
    Task PermanentlyDeleteProductAsync(ObjectId id);
    Task RestoreProductAsync(ObjectId id);
    Task CleanupOldArchivedProductsAsync();
    Task<ProductArchive> GetArchivedProductsAsync(ObjectId id);
    Task<IEnumerable<ProductArchive>> GetProductArchiveCollection(ObjectId sellerId);
}