using App.Core.Models.Product;
using System.Threading.Tasks;
using System.Collections.Generic;
using App.Core.DTOs.Product;

namespace App.Core.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>?> GetAllAsync(ProductFilterRequest filter);
    Task<ProductDto?> GetByIdAsync(string id);
    Task<List<ProductDto>?> GetByNameAsync(string name, ProductFilterRequest filter);
    Task<List<ProductDto>?> GetByCategoryAsync(string categoryId, ProductFilterRequest filter);
    Task<List<ProductDto>?> GetBySellerIdAsync(string sellerId, ProductFilterRequest filter);
    Task<ProductDto?> GetByModelIdAsync(string modelId, ProductFilterRequest filter);
    Task<List<ProductDto>?> GetByModelIdsAsync(List<string> modelId, ProductFilterRequest filter);
    Task<ProductDto> CreateAsync(ProductCreateDto productDto);
    Task<ProductDto?> UpdateAsync(ProductDto productDto);
    Task<bool> DeleteAsync(string id);
    Task<List<ProductSearchResult>?> SearchByNameAsync(string name, string languageCode = "en");

    // Variation management
    Task<ProductVariationDto?> AddVariationAsync(string productId, ProductVariationDto variationDto);
    Task<ProductVariationDto?> UpdateVariationAsync(string productId, ProductVariationDto variationDto);
    Task<bool> RemoveVariationAsync(string productId, string modelId);
} 