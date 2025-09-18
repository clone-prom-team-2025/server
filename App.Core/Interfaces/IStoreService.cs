using App.Core.DTOs.Store;
using App.Core.Enums;

namespace App.Core.Interfaces;

public interface IStoreService
{
    Task CreateRequest(CreateStoreCreateRequestDto dto, string userId, Stream stream, string fileName);
    Task DeleteRequest(string requestId);
    Task UpdateRequest(UpdateStoreCreateRequestDto dto);
    Task<IEnumerable<StoreCreateRequestDto>> GetAllRequests();
    Task<StoreCreateRequestDto?> GetRequestById(string requestId);
    Task<StoreCreateRequestDto?> GetRequestByUserId(string userId);
    Task<IEnumerable<StoreCreateRequestDto>> GetRequestApprovedByAdminId(string adminId);
    Task<IEnumerable<StoreCreateRequestDto>> GetRequestRejectedByAdminId(string adminId);
    Task ApproveRequest(string requestId, string adminId);
    Task RejectRequest(string requestId, string adminId);
    Task DeleteStore(string storeId, string userId);
    Task<StoreDto?> GetStoreById(string storeId);
    Task<IEnumerable<StoreDto>?> GetStores();
    Task AddMemberToStoreAsync(string userId, string memberId, StoreRole role);
    Task RemoveMemberFromStoreAsync(string userId, string memberId);
    Task<Dictionary<string, string>?> GetStoreMembers(string userId, string? storeId);
}