using App.Core.Models.Store;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IStoreCreateRequestRepository
{
    Task<bool> Create(StoreCreateRequest model);
    Task<bool> Update(StoreCreateRequest model);
    Task<bool> Delete(ObjectId requestId);
    Task<List<StoreCreateRequest>> GetAll();
    Task<StoreCreateRequest?> GetById(ObjectId id);
    Task<StoreCreateRequest> GetByUserId(ObjectId userId);
    Task<List<StoreCreateRequest>> GetApprovedByAdminId(ObjectId adminId);
    Task<List<StoreCreateRequest>> GetRejectedByAdminId(ObjectId adminId);
}