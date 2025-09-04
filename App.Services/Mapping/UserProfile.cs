using App.Core.DTOs.User;
using App.Core.Models.User;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<UserDto, User>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(dto =>
                    string.IsNullOrWhiteSpace(dto.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(dto.Id)));
    }
}