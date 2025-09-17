using App.Core.DTOs.Sell;
using App.Core.Models.Sell;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

public class BuyProfile : Profile
{
    public BuyProfile()
    {
        CreateMap<BuyInfo, BuyInfoDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()))
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.SellerId, o => o.MapFrom(s => s.SellerId.ToString()));
        
        CreateMap<BuyInfoDto, BuyInfo>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)))
            .ForMember(d => d.Id, o => o.MapFrom(s => ObjectId.Parse(s.Id)))
            .ForMember(d => d.SellerId, o => o.MapFrom(s => ObjectId.Parse(s.SellerId)));

        CreateMap<MiniProductInfo, MiniProductInfoDto>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId.ToString()));

        CreateMap<MiniProductInfoDto, MiniProductInfo>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => ObjectId.Parse(s.ProductId)));
    }
}