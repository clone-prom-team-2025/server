using App.Core.Constants;
using App.Core.DTOs.Store;
using App.Core.Enums;
using App.Core.Exceptions;
using App.Core.Interfaces;
using App.Core.Models.FileStorage;
using App.Core.Models.Store;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services.Services;

/// <summary>
/// Provides services for managing store creation requests, stores, and store members.
/// </summary>
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

    /// <summary>
    /// Creates a store creation request and uploads an avatar image.
    /// Throws InvalidOperationException if the uploaded file is not an image or creation fails.
    /// </summary>
    public async Task CreateRequest(CreateStoreCreateRequestDto dto, string userId, Stream stream,
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
                throw new InvalidOperationException("The uploaded file must be an image.");
            var result = await _requestRepository.Create(model);
            if (!result)
            {
                _logger.LogError("CreateStoreCreateRequestService failed to create store");
                throw new InvalidOperationException("Can't create store");
            }

            BaseFile file = new();
            (file.SourceUrl, file.CompressedUrl, file.SourceFileName, file.CompressedFileName) =
                await _fileService.SaveImageAsync(stream, fileName, "store-avatars");
            model.Avatar = file;
            var result2 = await _requestRepository.Update(model);
            if (!result2)
            {
                _logger.LogError("CreateStoreCreateRequestService failed to add store avatar");
                throw new InvalidOperationException("Can't add store avatar");
            }

            _logger.LogInformation("CreateStoreCreateRequestService succeeded");
        }
    }

    /// <summary>
    /// Deletes a store creation request and associated avatar files.
    /// Throws InvalidOperationException if deletion fails.
    /// </summary>
    public async Task DeleteRequest(string requestId)
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
                throw new InvalidOperationException("Can't delete store request");
            }

            _logger.LogInformation("DeleteStoreCreateRequestService succeeded");
        }
    }

    /// <summary>
    /// Updates an existing store creation request.
    /// Throws KeyNotFoundException if the request is not found.
    /// Throws InvalidOperationException if the update fails.
    /// </summary>
    public async Task UpdateRequest(UpdateStoreCreateRequestDto dto)
    {
        using (_logger.BeginScope("UpdateRequest"))
        {
            _logger.LogInformation("UpdateRequest called with dto");
            var request = await _requestRepository.GetById(ObjectId.Parse(dto.Id));
            if (request == null)
            {
                _logger.LogError("UpdateRequest can't find store request with Id={id}", dto.Id);
                throw new KeyNotFoundException("Store not found");
            }

            _logger.LogInformation("Fetched store request with Id={id}", dto.Id);
            request.Name = dto.Name;
            request.Plan = request.Plan;
            var updatedResult = await _requestRepository.Update(request);
            if (!updatedResult)
            {
                _logger.LogError("UpdateRequest failed to update store request");
                throw new InvalidOperationException("Can't update store request");
            }

            _logger.LogInformation("UpdateRequest succeeded");
        }
    }

    /// <summary>
    /// Retrieves all store creation requests.
    /// </summary>
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

    /// <summary>
    /// Retrieves a store creation request by its ID.
    /// Throws KeyNotFoundException if the request is not found.
    /// </summary>
    public async Task<StoreCreateRequestDto?> GetRequestById(string requestId)
    {
        using (_logger.BeginScope("GetRequestById"))
        {
            _logger.LogInformation("GetRequestById called with RequestId={requestId}", requestId);
            var result = await _requestRepository.GetById(ObjectId.Parse(requestId));
            if (result == null)
            {
                _logger.LogError("GetRequestById failed to get store request");
                throw new KeyNotFoundException("Store request not found");
            }

            _logger.LogInformation("GetRequestById succeeded");
            return _mapper.Map<StoreCreateRequestDto>(result);
        }
    }

    /// <summary>
    /// Retrieves a store creation request by user ID.
    /// Throws KeyNotFoundException if the request is not found.
    /// </summary>
    public async Task<StoreCreateRequestDto?> GetRequestByUserId(string userId)
    {
        using (_logger.BeginScope("GetRequestByUserId"))
        {
            _logger.LogInformation("GetRequestByUserId called with UserId={userId}", userId);
            var result = await _requestRepository.GetByUserId(ObjectId.Parse(userId));
            if (result == null)
            {
                _logger.LogError("GetRequestByUserId failed to get store request");
                throw new KeyNotFoundException("Store not found");
            }

            _logger.LogInformation("GetRequestByUserId succeeded");
            return _mapper.Map<StoreCreateRequestDto>(result);
        }
    }

    /// <summary>
    /// Retrieves all store requests approved by a specific admin.
    /// </summary>
    public async Task<IEnumerable<StoreCreateRequestDto>> GetRequestApprovedByAdminId(string adminId)
    {
        using (_logger.BeginScope("GetRequestApprovedByAdminId"))
        {
            _logger.LogInformation("GetRequestApprovedByAdminId called with adminId={adminId}", adminId);
            var result = await _requestRepository.GetApprovedByAdminId(ObjectId.Parse(adminId));
            if (result != null) _logger.LogDebug("Fetched {Count} requests for AdminId={UserId}", result.Count, adminId);
            _logger.LogInformation("GetRequestApprovedByAdminId succeeded");
            return _mapper.Map<IEnumerable<StoreCreateRequestDto>>(result);
        }
    }

    /// <summary>
    /// Retrieves all store requests rejected by a specific admin.
    /// </summary>
    public async Task<IEnumerable<StoreCreateRequestDto>> GetRequestRejectedByAdminId(string adminId)
    {
        using (_logger.BeginScope("GetRequestRejectedByAdminId"))
        {
            _logger.LogInformation("GetRequestRejectedByAdminId called with adminId={adminId}", adminId);
            var result = await _requestRepository.GetRejectedByAdminId(ObjectId.Parse(adminId));
            if (result != null) _logger.LogDebug("Fetched {Count} requests for AdminId={UserId}", result.Count, adminId);
            _logger.LogInformation("GetRequestRejectedByAdminId succeeded");
            return _mapper.Map<IEnumerable<StoreCreateRequestDto>>(result);
        }
    }

    /// <summary>
    /// Approves a store creation request and creates the store.
    /// Throws KeyNotFoundException if the request is not found.
    /// Throws InvalidOperationException if the request is already approved or update fails.
    /// </summary>
    public async Task ApproveRequest(string requestId, string adminId)
    {
        using (_logger.BeginScope("ApproveRequest"))
        {
            _logger.LogInformation("ApproveRequest called with Id={requestId} and AdminId={adminId}", requestId,
                adminId);
            var request = await _requestRepository.GetById(ObjectId.Parse(requestId));
            if (request == null)
            {
                _logger.LogError("ApproveRequest can't find request with Id={requestId}", requestId);
                throw new KeyNotFoundException("Request not found");
            }

            if (request.ApprovedByAdminId != null)
            {
                _logger.LogWarning("Already approved");
                throw new InvalidOperationException("Already approved");
            }

            if (request.RejectedByAdminId != null) request.RejectedByAdminId = null;

            request.ApprovedByAdminId = ObjectId.Parse(adminId);
            var result = await _requestRepository.Update(request);
            if (!result)
            {
                _logger.LogError("ApproveRequest failed to update request");
                throw new InvalidOperationException("Can't update request");
            }

            var store = new Store(request.Name, request.Avatar,
                new Dictionary<string, StoreRole> { { request.UserId.ToString(), StoreRole.Owner } }, request.Plan);
            await _storeRepository.CreateStore(store);

            _logger.LogInformation("Approved successfully for RequestId={requestId} by AdminId={adminId}", requestId,
                adminId);
            _logger.LogInformation("ApproveRequest succeeded");
        }
    }

    /// <summary>
    /// Rejects a store creation request.
    /// Throws KeyNotFoundException if the request is not found.
    /// Throws InvalidOperationException if the request is already approved, rejected, or update fails.
    /// </summary>
    public async Task RejectRequest(string requestId, string adminId)
    {
        using (_logger.BeginScope("RejectRequest"))
        {
            _logger.LogInformation("RejectRequest called with Id={requestId} and AdminId={adminId}", requestId,
                adminId);
            var request = await _requestRepository.GetById(ObjectId.Parse(requestId));
            if (request == null)
            {
                _logger.LogError("RejectRequest can't find request with Id={requestId}", requestId);
                throw new KeyNotFoundException("Request not found");
            }

            if (request.ApprovedByAdminId != null)
            {
                _logger.LogWarning("Already approved");
                throw new InvalidOperationException("Already approved");
            }

            if (request.RejectedByAdminId != null)
            {
                _logger.LogWarning("Already rejected");
                throw new InvalidOperationException("Already rejected");
            }

            request.RejectedByAdminId = ObjectId.Parse(adminId);
            var result = await _requestRepository.Update(request);
            if (!result)
            {
                _logger.LogError("RejectRequest failed to update request");
                throw new InvalidOperationException("Can't update request");
            }

            _logger.LogInformation("Rejected successfully for RequestId={requestId} by AdminId={adminId}", requestId,
                adminId);
            _logger.LogInformation("RejectRequest succeeded");
        }
    }

    /// <summary>
    /// Deletes a store if the user is the owner or an admin.
    /// Throws KeyNotFoundException if the user or store is not found.
    /// Throws AccessDeniedException if the user is not the owner or admin.
    /// Throws InvalidOperationException if deletion fails.
    /// </summary>
    public async Task DeleteStore(string storeId, string userId)
    {
        using (_logger.BeginScope("DeleteStore"))
        {
            _logger.LogInformation("DeleteStore called with StoreId={storeId} UserId={userId}", storeId, userId);
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogError("DeleteStore can't find user with Id={userId}", userId);
                throw new KeyNotFoundException("User not found");
            }

            var store = await _storeRepository.GetStoreById(ObjectId.Parse(storeId));
            if (store == null)
            {
                _logger.LogError("DeleteStore can't find request with Id={storeId}", storeId);
                throw new KeyNotFoundException("Store not found");
            }

            if (store.Roles[userId] != StoreRole.Owner && !user.Roles.Contains(RoleNames.Admin))
            {
                _logger.LogError("You are not the owner! UserId={userId} StoreId={storeId}", userId, storeId);
                throw new AccessDeniedException("You are not the owner");
            }

            var result = await _storeRepository.DeleteStore(ObjectId.Parse(storeId));
            if (!result)
            {
                _logger.LogError("DeleteStore failed to delete request with Id={storeId}", storeId);
                throw new InvalidOperationException("Can't delete request");
            }

            _logger.LogInformation("DeleteStore succeeded");
        }
    }

    /// <summary>
    /// Retrieves a store by its ID.
    /// </summary>
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

    /// <summary>
    /// Retrieves all stores.
    /// </summary>
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

    /// <summary>
    /// Adds a member to the store with a specific role.
    /// Throws InvalidOperationException if the user tries to add themselves or update fails.
    /// Throws KeyNotFoundException if the user, member, or store is not found.
    /// Throws AccessDeniedException if the user is not the store owner.
    /// </summary>
    public async Task AddMemberToStoreAsync(string userId, string memberId, StoreRole role)
    {
        using (_logger.BeginScope("AddMemberToStoreAsync"))
        {
            _logger.LogInformation("AddMemberToStoreAsync called");
            if (memberId == userId)
            {
                _logger.LogWarning("You cannot add yourself");
                throw new InvalidOperationException("You cannot add yourself");
            }

            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogError("AddMemberToStoreAsync can't find user with Id={userId}", userId);
                throw new KeyNotFoundException("User not found");
            }

            var store = await _storeRepository.GetStoreByUserId(ObjectId.Parse(userId));
            if (store == null)
            {
                _logger.LogError("Store not found");
                throw new KeyNotFoundException("Store not found");
            }

            if (!store.Roles.ContainsKey(userId) || store.Roles[userId] != StoreRole.Owner)
            {
                _logger.LogError("You are not the owner!");
                throw new AccessDeniedException("You are not the owner");
            }

            var member = await _userRepository.GetUserByIdAsync(ObjectId.Parse(memberId));
            if (member == null)
            {
                _logger.LogError("Member not found");
                throw new KeyNotFoundException("Member not found");
            }

            store.Roles.Remove(memberId);
            store.Roles.Add(member.Id.ToString(), role);
            var result = await _storeRepository.UpdateStore(store);
            if (!result)
            {
                _logger.LogError("AddMemberToStoreAsync failed to update store");
                throw new InvalidOperationException("Can't update store");
            }

            _logger.LogInformation("AddMemberToStoreAsync succeeded");
        }
    }

    /// <summary>
    /// Removes a member from the store.
    /// Throws InvalidOperationException if the user tries to remove themselves or update fails.
    /// Throws KeyNotFoundException if the user or store is not found.
    /// Throws AccessDeniedException if the user is not the store owner.
    /// </summary>
    public async Task RemoveMemberFromStoreAsync(string userId, string memberId)
    {
        using (_logger.BeginScope("RemoveMemberFromStoreAsync"))
        {
            _logger.LogInformation("RemoveMemberFromStoreAsync called");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogError("RemoveMemberFromStoreAsync can't find user with Id={userId}", userId);
                throw new KeyNotFoundException("User not found");
            }

            if (user.Id.ToString() == memberId)
            {
                _logger.LogError("You can't remove yourself");
                throw new InvalidOperationException("You can't remove yourself");
            }

            var store = await _storeRepository.GetStoreByUserId(ObjectId.Parse(userId));
            if (store == null)
            {
                _logger.LogError("Store not found");
                throw new KeyNotFoundException("Store not found");
            }

            if (!store.Roles.ContainsKey(userId) || store.Roles[userId] != StoreRole.Owner)
            {
                _logger.LogError("You are not the owner!");
                throw new AccessDeniedException("You are not the owner");
            }

            store.Roles.Remove(memberId);
            var result = await _storeRepository.UpdateStore(store);
            if (!result)
            {
                _logger.LogError("RemoveMemberFromStoreAsync failed to update store");
                throw new InvalidOperationException("Can't update store");
            }

            _logger.LogInformation("RemoveMemberFromStoreAsync succeeded");
        }
    }

    /// <summary>
    /// Retrieves store members with their roles.
    /// Throws KeyNotFoundException if the user or store is not found.
    /// Throws AccessDeniedException if the user is not the owner or admin.
    /// </summary>
    public async Task<Dictionary<string, string>?> GetStoreMembers(string userId, string? storeId)
    {
        using (_logger.BeginScope("GetStoreMembers"))
        {
            _logger.LogInformation("GetStoreMembers called");

            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));

            if (user == null)
            {
                _logger.LogError("GetStoreMembers can't find user with Id={userId}", userId);
                throw new KeyNotFoundException("User not found");
            }

            Store? store;
            if (storeId == null) store = await _storeRepository.GetStoreByUserId(user.Id);
            else store = await _storeRepository.GetStoreById(ObjectId.Parse(storeId));

            if (store == null)
            {
                _logger.LogError("Store not found");
                throw new KeyNotFoundException("Store not found");
            }

            if (!store.Roles.ContainsKey(userId) || user.Roles.Contains(RoleNames.Admin))
            {
                _logger.LogError("You are not the owner!");
                throw new AccessDeniedException("You are not the owner");
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