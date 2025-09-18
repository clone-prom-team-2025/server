using App.Core.Models.Store;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IStoreRepository
{
    Task CreateStore(Store model);
    Task<bool> UpdateStore(Store model);
    Task<bool> DeleteStore(ObjectId storeId);
    Task<Store?> GetStoreById(ObjectId storeId);
    Task<List<Store>?> GetStores();
    Task<Store?> GetStoreByUserId(ObjectId userId);
    Task<bool> ExistsByUserId(ObjectId userId);
    Task<bool> ExistsById(ObjectId id);
}