using App.Core.DTOs.Product.Review;
using App.Core.Models.Product.Review;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

public class ProductReviewProfile : Profile
{
    public ProductReviewProfile()
    {
        CreateMap<ProductReview, ProductReviewDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId.ToString()));

        CreateMap<ProductReviewDto, ProductReview>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => ObjectId.Parse(s.ProductId)))
            .ForMember(d => d.Id, o => o.MapFrom(s => ObjectId.Parse(s.Id)));

        CreateMap<ProductReviewComment, ProductReviewCommentDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()))
            .ForMember(d => d.Reactions, o => o.MapFrom(s =>
                s.Reactions.ToDictionary(
                    kv => kv.Key.ToString(),
                    kv => kv.Value
                )));

        CreateMap<ProductReviewCommentDto, ProductReviewComment>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)))
            .ForMember(d => d.Reactions, o => o.MapFrom(s =>
                s.Reactions.ToDictionary(
                    kv => ObjectId.Parse(kv.Key),
                    kv => kv.Value
                )));

        CreateMap<ProductReviewCommentCreateDto, ProductReviewComment>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)));
    }
}