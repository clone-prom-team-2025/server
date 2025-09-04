using App.Core.DTOs.Store;
using App.Core.Models.Store;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IStoreService
{
    Task<bool> CreateRequest(CreateStoreCreateRequestDto dto, string userId, Stream stream, string fileName);
    Task<bool> DeleteRequest(string requestId);
    Task<bool> UpdateRequest(UpdateStoreCreateRequestDto dto);
    Task<IEnumerable<StoreCreateRequestDto>> GetAllRequests();
    Task<StoreCreateRequestDto?> GetRequestById(string requestId);
    Task<StoreCreateRequestDto?> GetRequestByUserId(string userId);
    Task<IEnumerable<StoreCreateRequestDto>> GetRequestApprovedByAdminId(string adminId);
    Task<IEnumerable<StoreCreateRequestDto>> GetRequestRejectedByAdminId(string adminId);
    Task<bool> ApproveRequest(string requestId, string adminId);
    Task<bool> RejectRequest(string requestId, string adminId);
    
    Task<bool> DeleteStore(string storeId, string userId);
    Task<StoreDto?> GetStoreById(string storeId);
    Task<IEnumerable<StoreDto>?> GetStores();
}