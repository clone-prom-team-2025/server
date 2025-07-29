using App.Core.DTOs.Product;
using App.Core.Models.Product;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

/// <summary>
/// AutoMapper profile for mapping between Product-related domain models and DTOs.
/// Handles transformations for <see cref="Product"/>, <see cref="ProductDto"/>, 
/// <see cref="ProductCreateDto"/>, <see cref="ProductMedia"/>, <see cref="ProductMediaDto"/>,
/// <see cref="ProductFeature"/>, and related types.
/// </summary>
public class ProductProfile : Profile
{
    private const string RootPrefix = "wwwroot/";

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductProfile"/> class.
    /// Configures mapping rules for product entities and their corresponding DTOs.
    /// </summary>
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
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId.ToString()))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src =>
                TrimRoot(src.Url)))
            .ForMember(dest => dest.SecondaryUrl, opt => opt.MapFrom(src =>
                TrimRoot(src.SecondaryUrl)));

        CreateMap<ProductMediaDto, ProductMedia>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(dto =>
                string.IsNullOrWhiteSpace(dto.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(dto.Id)))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(dto => ObjectId.Parse(dto.ProductId)))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(dto => RootPrefix + dto.Url))
            .ForMember(dest => dest.SecondaryUrl, opt => opt.MapFrom(dto =>
                string.IsNullOrWhiteSpace(dto.SecondaryUrl) ? null : RootPrefix + dto.SecondaryUrl));


        CreateMap<ProductMediaCreateDto, ProductMedia>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(dto => ObjectId.Parse(dto.ProductId)));

        CreateMap<ProductFeatureDto, ProductFeature>()
            .ConstructUsing(dto => new ProductFeature(dto.Category))
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features));

        CreateMap<ProductFeature, ProductFeatureDto>()
            .ConstructUsing(model => new ProductFeatureDto(model.Category))
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features));

        CreateMap<ProductFeatureItemDto, ProductFeatureItem>()
            .ConstructUsing(dto => new ProductFeatureItem(dto.Value, dto.Type, dto.Nullable));

        CreateMap<ProductFeatureItem, ProductFeatureItemDto>()
            .ConstructUsing(model => new ProductFeatureItemDto(model.Value, model.Type, model.Nullable));
    }

    /// <summary>
    /// Removes the 'wwwroot/' prefix from a given path if it exists.
    /// Used to clean up media URLs when mapping to DTOs.
    /// </summary>
    /// <param name="path">The path to process.</param>
    /// <returns>The trimmed path, or the original path if no prefix is found.</returns>
    string? TrimRoot(string? path) =>
            !string.IsNullOrWhiteSpace(path) && path.StartsWith(RootPrefix)
                ? path.Substring(RootPrefix.Length)
                : path;
}