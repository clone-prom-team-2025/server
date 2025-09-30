using App.Core.Archive.Product;
using App.Core.Archive.Product.DTOs;
using App.Core.Models.Product;
using App.Core.Models.Product.Review;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

public class ArchiveProfile : Profile
{
    public ArchiveProfile()
    {
        CreateMap<ProductArchive, Product>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId));

        CreateMap<Product, ProductArchive>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id));

        CreateMap<ProductMediaArchive, ProductMedia>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductMediaId));

        CreateMap<ProductMedia, ProductMediaArchive>()
            .ForMember(dest => dest.ProductMediaId, opt => opt.MapFrom(src => src.Id));

        CreateMap<ProductReviewArchive, ProductReview>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductReviewId));

        CreateMap<ProductReview, ProductReviewArchive>()
            .ForMember(dest => dest.ProductReviewId, opt => opt.MapFrom(src => src.Id));

        CreateMap<ProductArchive, ProductArchiveDto>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId.ToString()))
            .ForMember(d => d.SellerId, o => o.MapFrom(s => s.SellerId.ToString()))
            .ForMember(d => d.CategoryPath, o => o.MapFrom(s => s.CategoryPath.Select(c => c.ToString()).ToList()));

        CreateMap<ProductArchiveDto, ProductArchive>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => ObjectId.Parse(s.ProductId)))
            .ForMember(d => d.SellerId, o => o.MapFrom(s => ObjectId.Parse(s.SellerId)))
            .ForMember(d => d.CategoryPath, o => o.MapFrom(s => s.CategoryPath.Select(ObjectId.Parse).ToList()));
    }
}