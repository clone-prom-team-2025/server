using App.Core.Constants;
using App.Core.DTOs.Store;
using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.FileStorage;
using App.Core.Models.Store;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services;

public class StoreService(
    IStoreCreateRequestRepository requestRepository,
    ILogger<StoreService> logger,
    IMapper mapper,
    IStoreRepository storeRepository,
    IFileService fileService,
    IUserRepository userRepository)
    : IStoreService
{
    private readonly IFileService _fileService = fileService;
    private readonly ILogger<StoreService> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IStoreCreateRequestRepository _requestRepository = requestRepository;
    private readonly IStoreRepository _storeRepository = storeRepository;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<bool> CreateRequest(CreateStoreCreateRequestDto dto, string userId, Stream stream,
        string fileName)
    {
        using (_logger.BeginScope("CreateStoreCreateRequestService"))
        {
            _logger.LogInformation("CreateStoreCreateRequestService called with dto");
            var model = _mapper.Map<StoreCreateRequest>(dto);
            model.Id = ObjectId.GenerateNewId();
            model.UserId = ObjectId.Parse(userId);
            model.CreatedAt = DateTime.UtcNow;

            var type = MediaInspector.GetMediaType(stream, fileName);
            if (type == MediaType.Unknown || type == MediaType.Video)
                return false;
            var result = await _requestRepository.Create(model);
            if (!result)
            {
                _logger.LogError("CreateStoreCreateRequestService failed to create store");
                return false;
            }

            BaseFile file = new();
            (file.SourceUrl, file.CompressedUrl, file.SourceFileName, file.CompressedFileName) =
                await _fileService.SaveImageAsync(stream, fileName, "store-avatars");
            model.Avatar = file;
            var result2 = await _requestRepository.Update(model);
            if (!result2)
            {
                _logger.LogError("CreateStoreCreateRequestService failed to add store avatar");
                return false;
            }

            _logger.LogInformation("CreateStoreCreateRequestService succeeded");
            return true;
        }
    }

    public async Task<bool> DeleteRequest(string requestId)
    {
        using (_logger.BeginScope("DeleteStoreCreateRequestService"))
        {
            _logger.LogInformation("DeleteStoreCreateRequestService called with RequestId={requestId}", requestId);
            var request = await _requestRepository.GetById(ObjectId.Parse(requestId));
            if (request != null)
            {
                await _fileService.DeleteFileAsync("store-avatars", request.Avatar.SourceFileName);
                await _fileService.DeleteFileAsync("store-avatars", request.Avatar.CompressedFileName!);
            }

            var result = await _requestRepository.Delete(ObjectId.Parse(requestId));
            if (!result)
            {
                _logger.LogError("DeleteStoreCreateRequestService failed to delete store request");
                return false;
            }

            _logger.LogInformation("DeleteStoreCreateRequestService succeeded");
            return true;
        }
    }

    public async Task<bool> UpdateRequest(UpdateStoreCreateRequestDto dto)
    {
        using (_logger.BeginScope("UpdateRequest"))
        {
            _logger.LogInformation("UpdateRequest called with dto");
            var request = await _requestRepository.GetById(ObjectId.Parse(dto.Id));
            if (request == null)
            {
                _logger.LogError("UpdateRequest can't find store request with Id={id}", dto.Id);
                return false;
            }

            _logger.LogInformation("Fetched store request with Id={id}", dto.Id);
            request.Name = dto.Name;
            request.Plan = request.Plan;
            var updatedResult = await _requestRepository.Update(request);
            if (!updatedResult)
            {
                _logger.LogError("UpdateRequest failed to update store request");
                return false;
            }

            _logger.LogInformation("UpdateRequest succeeded");
            return true;
        }
    }

    public async Task<IEnumerable<StoreCreateRequestDto>> GetAllRequests()
    {
        using (_logger.BeginScope("GetAllRequests"))
        {
            _logger.LogInformation("GetAllRequests called");
            var result = await _requestRepository.GetAll();
            _logger.LogInformation("GetAllRequests succeeded");
            return _mapper.Map<IEnumerable<StoreCreateRequestDto>>(result);
        }
    }

    public async Task<StoreCreateRequestDto?> GetRequestById(string requestId)
    {
        using (_logger.BeginScope("GetRequestById"))
        {
            _logger.LogInformation("GetRequestById called with RequestId={requestId}", requestId);
            var result = await _requestRepository.GetById(ObjectId.Parse(requestId));
            if (result == null)
            {
                _logger.LogError("GetRequestById failed to get store request");
                return null;
            }

            _logger.LogInformation("GetRequestById succeeded");
            return _mapper.Map<StoreCreateRequestDto>(result);
        }
    }

    public async Task<StoreCreateRequestDto?> GetRequestByUserId(string userId)
    {
        using (_logger.BeginScope("GetRequestByUserId"))
        {
            _logger.LogInformation("GetRequestByUserId called with UserId={userId}", userId);
            var result = await _requestRepository.GetByUserId(ObjectId.Parse(userId));
            if (result == null)
            {
                _logger.LogError("GetRequestByUserId failed to get store request");
                return null;
            }

            _logger.LogInformation("GetRequestByUserId succeeded");
            return _mapper.Map<StoreCreateRequestDto>(result);
        }
    }

    public async Task<IEnumerable<StoreCreateRequestDto>> GetRequestApprovedByAdminId(string adminId)
    {
        using (_logger.BeginScope("GetRequestApprovedByAdminId"))
        {
            _logger.LogInformation("GetRequestApprovedByAdminId called with adminId={adminId}", adminId);
            var result = await _requestRepository.GetApprovedByAdminId(ObjectId.Parse(adminId));
            _logger.LogDebug("Fetched {Count} requests for AdminId={UserId}", result.Count, adminId);
            _logger.LogInformation("GetRequestApprovedByAdminId succeeded");
            return _mapper.Map<IEnumerable<StoreCreateRequestDto>>(result);
        }
    }

    public async Task<IEnumerable<StoreCreateRequestDto>> GetRequestRejectedByAdminId(string adminId)
    {
        using (_logger.BeginScope("GetRequestRejectedByAdminId"))
        {
            _logger.LogInformation("GetRequestRejectedByAdminId called with adminId={adminId}", adminId);
            var result = await _requestRepository.GetRejectedByAdminId(ObjectId.Parse(adminId));
            _logger.LogDebug("Fetched {Count} requests for AdminId={UserId}", result.Count, adminId);
            _logger.LogInformation("GetRequestRejectedByAdminId succeeded");
            return _mapper.Map<IEnumerable<StoreCreateRequestDto>>(result);
        }
    }

    public async Task<bool> ApproveRequest(string requestId, string adminId)
    {
        using (_logger.BeginScope("ApproveRequest"))
        {
            _logger.LogInformation("ApproveRequest called with Id={requestId} and AdminId={adminId}", requestId,
                adminId);
            var request = await _requestRepository.GetById(ObjectId.Parse(requestId));
            if (request == null)
            {
                _logger.LogError("ApproveRequest can't find request with Id={requestId}", requestId);
                return false;
            }

            if (request.ApprovedByAdminId != null)
            {
                _logger.LogWarning("Already approved");
                return false;
            }

            if (request.RejectedByAdminId != null)
            {
                _logger.LogWarning("Already rejected");
                return false;
            }

            request.ApprovedByAdminId = ObjectId.Parse(adminId);
            var result = await _requestRepository.Update(request);
            if (!result)
            {
                _logger.LogError("ApproveRequest failed to update request");
                return false;
            }

            var store = new Store(request.Name, request.Avatar,
                new Dictionary<string, StoreRole> { { request.UserId.ToString(), StoreRole.Owner } }, request.Plan);
            await _storeRepository.CreateStore(store);

            _logger.LogInformation("Approved successfully for RequestId={requestId} by AdminId={adminId}", requestId,
                adminId);
            _logger.LogInformation("ApproveRequest succeeded");
            return true;
        }
    }

    public async Task<bool> RejectRequest(string requestId, string adminId)
    {
        using (_logger.BeginScope("RejectRequest"))
        {
            _logger.LogInformation("RejectRequest called with Id={requestId} and AdminId={adminId}", requestId,
                adminId);
            var request = await _requestRepository.GetById(ObjectId.Parse(requestId));
            if (request == null)
            {
                _logger.LogError("RejectRequest can't find request with Id={requestId}", requestId);
                return false;
            }

            if (request.ApprovedByAdminId != null)
            {
                _logger.LogWarning("Already approved");
                return false;
            }

            if (request.RejectedByAdminId != null)
            {
                _logger.LogWarning("Already rejected");
                return false;
            }

            request.RejectedByAdminId = ObjectId.Parse(adminId);
            var result = await _requestRepository.Update(request);
            if (!result)
            {
                _logger.LogError("RejectRequest failed to update request");
                return false;
            }

            _logger.LogInformation("Rejected successfully for RequestId={requestId} by AdminId={adminId}", requestId,
                adminId);
            _logger.LogInformation("RejectRequest succeeded");
            return true;
        }
    }

    public async Task<bool> DeleteStore(string storeId, string userId)
    {
        using (_logger.BeginScope("DeleteStore"))
        {
            _logger.LogInformation("DeleteStore called with StoreId={storeId} UserId={userId}", storeId, userId);
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogError("DeleteStore can't find user with Id={userId}", userId);
                return false;
            }

            var store = await _storeRepository.GetStoreById(ObjectId.Parse(storeId));
            if (store == null)
            {
                _logger.LogError("DeleteStore can't find request with Id={storeId}", storeId);
                return false;
            }

            if (store.Roles[userId] != StoreRole.Owner && !user.Roles.Contains(RoleNames.Admin))
            {
                _logger.LogError("You are not the owner! UserId={userId} StoreId={storeId}", userId, storeId);
                return false;
            }

            var result = await _storeRepository.DeleteStore(ObjectId.Parse(storeId));
            if (!result)
            {
                _logger.LogError("DeleteStore failed to delete request with Id={storeId}", storeId);
                return false;
            }

            _logger.LogInformation("DeleteStore succeeded");
            return true;
        }
    }

    public async Task<StoreDto?> GetStoreById(string storeId)
    {
        using (_logger.BeginScope("GetStoreById"))
        {
            _logger.LogInformation("GetStoreById called with Id={storeId}", storeId);
            var result = await _storeRepository.GetStoreById(ObjectId.Parse(storeId));
            _logger.LogInformation("GetStoreById succeeded");
            return _mapper.Map<StoreDto?>(result);
        }
    }

    public async Task<IEnumerable<StoreDto>?> GetStores()
    {
        using (_logger.BeginScope("GetStores"))
        {
            _logger.LogInformation("GetStores called");
            var result = await _storeRepository.GetStores();
            _logger.LogInformation("GetStores succeeded");
            return _mapper.Map<IEnumerable<StoreDto>>(result);
        }
    }

    public async Task<bool> AddMemberToStoreAsync(string userId, string memberId, StoreRole role)
    {
        using (_logger.BeginScope("AddMemberToStoreAsync"))
        {
            _logger.LogInformation("AddMemberToStoreAsync called");
            if (memberId == userId)
            {
                return false;
            }
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogError("AddMemberToStoreAsync can't find user with Id={userId}", userId);
                return false;
            }
            var store = await _storeRepository.GetStoreByUserId(ObjectId.Parse(userId));
            if (store == null)
            {
                _logger.LogError("Store not found");
                return false;
            }
            if (!store.Roles.ContainsKey(userId) || store.Roles[userId] != StoreRole.Owner)
            {
                _logger.LogError("You are not the owner!");
                return false;
            }
            var member = await _userRepository.GetUserByIdAsync(ObjectId.Parse(memberId));
            if (member == null)
            {
                _logger.LogError("Member not found");
                return false;
            }
            store.Roles.Remove(memberId);
            store.Roles.Add(member.Id.ToString(), role);
            var result = await _storeRepository.UpdateStore(store);
            if (!result)
            {
                _logger.LogError("AddMemberToStoreAsync failed to update store");
                return false;
            }
            _logger.LogInformation("AddMemberToStoreAsync succeeded");
            return true;
        }
    }
    
    public async Task<bool> RemoveMemberFromStoreAsync(string userId, string memberId)
    {
        using (_logger.BeginScope("RemoveMemberFromStoreAsync"))
        {
            _logger.LogInformation("RemoveMemberFromStoreAsync called");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogError("RemoveMemberFromStoreAsync can't find user with Id={userId}", userId);
                return false;
            }
            if (user.Id.ToString() == memberId)
            {
                _logger.LogError("You can't remove yourself");
                return false;
            }
            var store = await _storeRepository.GetStoreByUserId(ObjectId.Parse(userId));
            if (store == null)
            {
                _logger.LogError("Store not found");
                return false;
            }
            if (!store.Roles.ContainsKey(userId) || store.Roles[userId] != StoreRole.Owner)
            {
                _logger.LogError("You are not the owner!");
                return false;
            }
            
            store.Roles.Remove(memberId);
            var result = await _storeRepository.UpdateStore(store);
            if (!result)
            {
                _logger.LogError("RemoveMemberFromStoreAsync failed to update store");
                return false;
            }
            _logger.LogInformation("RemoveMemberFromStoreAsync succeeded");
            return true;
        }
    }

    public async Task<Dictionary<string, string>?> GetStoreMembers(string userId, string? storeId)
    {
        using (_logger.BeginScope("GetStoreMembers"))
        {
            _logger.LogInformation("GetStoreMembers called");
            
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            
            if (user == null)
            {
                _logger.LogError("GetStoreMembers can't find user with Id={userId}", userId);
                return null;
            }
            Store? store = null;
            if (storeId == null) store = await _storeRepository.GetStoreByUserId(user.Id);
            else store = await _storeRepository.GetStoreById(ObjectId.Parse(storeId));
            
            if (store == null)
            {
                _logger.LogError("Store not found");
                return null;
            }
            
            if (!store.Roles.ContainsKey(userId) && user.Roles.Contains(RoleNames.Admin))
            {
                _logger.LogError("You are not the owner!");
                return null;
            }
            
            var result = store.Roles.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToString()
            );
            
            _logger.LogInformation("GetStoreMembers succeeded");
            
            return result;
        }
    }
}