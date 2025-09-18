using App.Core.DTOs.Cart;
using App.Core.Models.Cart;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<Cart, CartDto>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId.ToString()))
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()));

        CreateMap<CartDto, Cart>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => ObjectId.Parse(s.ProductId)))
            .ForMember(d => d.Id, o => o.MapFrom(s => ObjectId.Parse(s.Id)))
            .ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)));

        CreateMap<CreateCartDto, Cart>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId))
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore());
    }
}