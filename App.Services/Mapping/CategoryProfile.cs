using App.Core.DTOs.Categoty;
using App.Core.Models;
using AutoMapper;
using MongoDB.Bson;

namespace App.Core.Mapping;

/// <summary>
/// AutoMapper profile for mapping between category-related DTOs and domain models.
/// </summary>  
public class CategoryProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryProfile"/> class
    /// and defines mappings between <see cref="Category"/>, <see cref="CategoryDto"/>, and <see cref="CategoryCreateDto"/>.
    /// </summary>
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(
                dest => dest.ParentId,
                opt => opt.MapFrom(src => src.ParentId.HasValue ? src.ParentId.ToString() : null)
            )
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => src.Id.ToString())
            );

        CreateMap<CategoryDto, Category>()
            .ForMember(
                dest => dest.ParentId,
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ParentId) ? new ObjectId(src.ParentId) : (ObjectId?)null)
            )
            .ForMember(
                dest => dest.Id,
                opt => opt.Ignore()
            );

        CreateMap<CategoryCreateDto, Category>()
            .ConstructUsing(dto =>
                new Category(
                    dto.Name,
                    !string.IsNullOrEmpty(dto.ParentId) ? new ObjectId(dto.ParentId) : (ObjectId?)null
                )
            );
    }
}