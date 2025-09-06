using App.Core.DTOs.Auth;
using App.Core.Models.Auth;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<UserSession, UserSessionDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()))
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()));
        
        CreateMap<UserSessionDto, UserSession>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)))
            .ForMember(d => d.Id, o => o.MapFrom(s => ObjectId.Parse(s.Id)));
    }
}