using App.Core.DTOs.Store;
using App.Core.Interfaces;
using App.Core.Models.Store;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services;

public class StoreService(IStoreCreateRequestRepository requestRepository, ILogger<StoreService> logger, IMapper mapper)
    : IStoreService
{
    private readonly ILogger<StoreService> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IStoreCreateRequestRepository _requestRepository = requestRepository;

    public async Task<bool> CreateRequest(CreateStoreCreateRequestDto dto, string userId)
    {
        using (_logger.BeginScope("CreateStoreCreateRequestService"))
        {
            _logger.LogInformation("CreateStoreCreateRequestService called with dto");
            var model = _mapper.Map<StoreCreateRequest>(dto);
            model.Id = ObjectId.GenerateNewId();
            model.UserId = ObjectId.Parse(userId);
            model.CreatedAt = DateTime.UtcNow;
            var result = await _requestRepository.Create(model);
            if (!result)
            {
                _logger.LogError("CreateStoreCreateRequestService failed to create store");
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

            request.ApprovedByAdminId = ObjectId.Parse(adminId);
            var result = await _requestRepository.Update(request);
            if (!result)
            {
                _logger.LogError("ApproveRequest failed to update request");
                return false;
            }

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
}