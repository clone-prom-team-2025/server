using App.Core.DTOs.User;
using App.Core.Models.User;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

public class BanUserProfile : Profile
{
    public BanUserProfile()
    {
        CreateMap<UserBan, UserBanDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.AdminId, opt => opt.MapFrom(src => src.AdminId.ToString()));
        
        CreateMap<UserBanDto, UserBan>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => ObjectId.Parse(src.UserId)))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)))
            .ForMember(dest => dest.AdminId, opt => opt.MapFrom(src => ObjectId.Parse(src.AdminId)));
    }
}