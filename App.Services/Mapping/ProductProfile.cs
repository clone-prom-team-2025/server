using App.Core.DTOs.Product;
using App.Core.Models.Product;
using AutoMapper;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace App.Services.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductFeatureItem, ProductFeatureDto>().ReverseMap();

        CreateMap<ProductVariation, ProductVariationDto>()
            .ReverseMap();

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.SellerId.ToString()))
            .ForMember(dest => dest.CategoryPath, opt => opt.MapFrom(src => src.CategoryPath.Select(oid => oid.ToString()).ToList()));

        CreateMap<ProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(dto => string.IsNullOrWhiteSpace(dto.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(dto.Id)))
            .ForMember(dest => dest.SellerId, opt => opt.MapFrom(dto => ObjectId.Parse(dto.SellerId)))
            .ForMember(dest => dest.CategoryPath, opt => opt.MapFrom(dto => dto.CategoryPath.Select(ObjectId.Parse).ToList()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(dto => dto.Name))
            .ForMember(dest => dest.ProductType, opt => opt.MapFrom(dto => dto.ProductType))
            .ForMember(dest => dest.Variations, opt => opt.MapFrom(dto => dto.Variations));

        CreateMap<ProductCreateDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SellerId, opt => opt.MapFrom(dto => ObjectId.Parse(dto.SellerId)))
            .ForMember(dest => dest.CategoryPath, opt => opt.MapFrom(dto => dto.CategoryPath.Select(ObjectId.Parse).ToList()))
            .ForMember(dest => dest.Variations, opt => opt.MapFrom(dto => dto.Variations))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(dto => dto.Name))
            .ForMember(dest => dest.ProductType, opt => opt.MapFrom(dto => dto.ProductType));

        CreateMap<ProductMedia, ProductMediaDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId.ToString()));

        CreateMap<ProductMediaDto, ProductMedia>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(dto => string.IsNullOrWhiteSpace(dto.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(dto.Id)))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(dto => ObjectId.Parse(dto.ProductId)));

        CreateMap<ProductMediaCreateDto, ProductMedia>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(dto => ObjectId.Parse(dto.ProductId)));

        // Deleted unused mappings
    }
}