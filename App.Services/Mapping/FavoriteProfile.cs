using App.Core.DTOs.Favorite;
using App.Core.Models.Favorite;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

public class FavoriteProfile : Profile
{
    public FavoriteProfile()
    {
        CreateMap<FavoriteProductDto, FavoriteProduct>()
            .ForMember(d => d.Id, o => o.MapFrom(s => ObjectId.Parse(s.Id)))
            .ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)))
            .ForMember(d => d.Products, o => o.MapFrom(s => s.Products.Select(ObjectId.Parse).ToList()));

        CreateMap<FavoriteProductCreateDto, FavoriteProduct>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)))
            .ForMember(d => d.Products, o => o.MapFrom(s => s.Products.Select(ObjectId.Parse).ToList()));

        CreateMap<FavoriteProduct, FavoriteProductDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()))
            .ForMember(d => d.Products, o => o.MapFrom(s => s.Products.Select(id => id.ToString()).ToList()));

        CreateMap<FavoriteSellerDto, FavoriteSeller>()
            .ForMember(d => d.Id, o => o.MapFrom(s => ObjectId.Parse(s.Id)))
            .ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)))
            .ForMember(d => d.Sellers, o => o.MapFrom(s => s.Sellers.Select(ObjectId.Parse).ToList()));

        CreateMap<FavoriteSellerCreateDto, FavoriteSeller>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)))
            .ForMember(d => d.Sellers, o => o.MapFrom(s => s.Sellers.Select(ObjectId.Parse).ToList()));

        CreateMap<FavoriteSeller, FavoriteSellerDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()))
            .ForMember(d => d.Sellers, o => o.MapFrom(s => s.Sellers.Select(id => id.ToString()).ToList()));
    }
}