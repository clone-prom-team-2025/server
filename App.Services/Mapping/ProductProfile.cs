using App.Core.DTOs.AvailableFilters;
using App.Core.DTOs.Product;
using App.Core.DTOs.Product.Review;
using App.Core.Models.AvailableFilters;
using App.Core.Models.Product;
using App.Core.Models.Product.Review;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

/// <summary>
///     AutoMapper profile for mapping between Product-related domain models and DTOs.
///     Handles transformations for <see cref="Product" />, <see cref="ProductDto" />,
///     <see cref="ProductCreateDto" />, <see cref="ProductMedia" />, <see cref="ProductMediaDto" />,
///     <see cref="ProductFeature" />, and related types.
/// </summary>
public class ProductProfile : Profile
{
    private const string RootPrefix = "wwwroot/";

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProductProfile" /> class.
    ///     Configures mapping rules for product entities and their corresponding DTOs.
    /// </summary>
    public ProductProfile()
    {
        CreateMap<ProductFeatureItem, ProductFeatureDto>().ReverseMap();


        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.SellerId.ToString()))
            .ForMember(dest => dest.CategoryPath,
                opt => opt.MapFrom(src => src.CategoryPath.Select(oid => oid.ToString()).ToList()));

        CreateMap<ProductDto, Product>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(dto =>
                    string.IsNullOrWhiteSpace(dto.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(dto.Id)))
            .ForMember(dest => dest.SellerId, opt => opt.MapFrom(dto => ObjectId.Parse(dto.SellerId)))
            .ForMember(dest => dest.CategoryPath,
                opt => opt.MapFrom(dto => dto.CategoryPath.Select(ObjectId.Parse).ToList()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(dto => dto.Name))
            .ForMember(dest => dest.Features, opt => opt.MapFrom(dto => dto.Features));

        CreateMap<ProductCreateDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SellerId, opt => opt.MapFrom(dto => ObjectId.Parse(dto.SellerId)))
            .ForMember(dest => dest.CategoryPath, opt => opt.Ignore())
            .ForMember(dest => dest.Features, opt => opt.MapFrom(dto => dto.Features))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(dto => dto.Name));

        CreateMap<ProductMedia, ProductMediaDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId.ToString()));

        CreateMap<ProductMediaDto, ProductMedia>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(dto =>
                string.IsNullOrWhiteSpace(dto.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(dto.Id)))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(dto => ObjectId.Parse(dto.ProductId)))
            .ForMember(dest => dest.Files, opt => opt.MapFrom(dto => RootPrefix + dto.Files));


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

        CreateMap<ProductReview, ProductReviewDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId.ToString()))
            .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.ModelId))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments));

        CreateMap<ProductReviewDto, ProductReview>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(dto => ObjectId.Parse(dto.Id)))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(dto => ObjectId.Parse(dto.ProductId)))
            .ForMember(dest => dest.ModelId, opt => opt.MapFrom(dto => dto.ModelId))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(dto => dto.Comments));

        CreateMap<ProductReviewComment, ProductReviewCommentDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
            .ForMember(dest => dest.Reactions, opt => opt.MapFrom(src => src.Reactions));

        CreateMap<ProductReviewCommentDto, ProductReviewComment>()
            .ConstructUsing(dto =>
                new ProductReviewComment(dto.Rating, ObjectId.Parse(dto.UserId), dto.Comment))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(dto => dto.Id))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(dto => dto.CreatedAt))
            .ForMember(dest => dest.Reactions, opt => opt.MapFrom(dto => dto.Reactions));

        CreateMap<ProductReviewCommentReaction, ProductReviewCommentReactionDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()));

        CreateMap<ProductReviewCommentReactionDto, ProductReviewCommentReaction>()
            .ConstructUsing(dto => new ProductReviewCommentReaction(ObjectId.Parse(dto.UserId), dto.Positive));

        CreateMap<ProductReviewCreateDto, ProductReview>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(dto => ObjectId.Parse(dto.ProductId)))
            .ForMember(dest => dest.ModelId, opt => opt.MapFrom(dto => dto.ModelId))
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(dto => dto.AverageRating))
            .ForMember(dest => dest.Comments, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<ProductReviewCommentCreateDto, ProductReviewComment>()
            .ConstructUsing(dto =>
                new ProductReviewComment(dto.Rating, ObjectId.Parse(dto.UserId), dto.Comment))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Reactions, opt => opt.Ignore());

        CreateMap<ProductFilterRequest, ProductFilterRequestDto>()
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId.ToString()));

        CreateMap<ProductFilterRequestDto, ProductFilterRequest>()
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => ObjectId.Parse(src.CategoryId)));

        CreateMap<AvailableFilters, AvailableFiltersDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId.ToString()));

        CreateMap<AvailableFiltersDto, AvailableFilters>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => ObjectId.Parse(src.CategoryId)));

        CreateMap<AvailableFiltersCreateDto, AvailableFilters>()
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => ObjectId.Parse(src.CategoryId)))
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<AvailableFiltersItem, AvailableFiltersItemDto>().ReverseMap();

        CreateMap<ProductSearchResult, ProductSearchResultDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId.ToString()));

        CreateMap<ProductSearchResultDto, ProductSearchResult>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => ObjectId.Parse(src.ProductId)));
    }
}