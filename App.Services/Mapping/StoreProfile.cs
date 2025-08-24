using App.Core.DTOs.Store;
using App.Core.Models.Store;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

public class StoreProfile : Profile
{
    public StoreProfile()
    {
        CreateMap<CreateStoreCreateRequestDto, StoreCreateRequest>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedByAdminId,
                opt => opt.Ignore())
            .ForMember(dest => dest.RejectedByAdminId,
                opt => opt.Ignore());

        CreateMap<StoreCreateRequest, StoreCreateRequestDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
            .ForMember(dest => dest.ApprovedByAdminId, opt => opt.MapFrom(src => src.ApprovedByAdminId.ToString()))
            .ForMember(dest => dest.RejectedByAdminId, opt => opt.MapFrom(src => src.RejectedByAdminId.ToString()));

        CreateMap<StoreRequestCommentDto, StoreRequestComment>()
            .ForMember(
                dest => dest.UserId,
                opt => opt.MapFrom(src => ObjectId.Parse(src.UserId))
            );

        CreateMap<StoreRequestComment, StoreRequestCommentDto>()
            .ForMember(
                dest => dest.UserId,
                opt => opt.MapFrom(src => src.UserId.ToString())
            );
    }
}