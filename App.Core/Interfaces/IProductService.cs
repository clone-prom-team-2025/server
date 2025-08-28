using App.Core.DTOs.Product;

namespace App.Core.Interfaces;

public interface IProductService
{
    Task<ProductFilterResponseDto?> GetAllAsync(ProductFilterRequestDto filter);
    Task<ProductDto?> GetByIdAsync(string id);

    Task<ProductFilterResponseDto?> GetByNameAsync(string name, ProductFilterRequestDto filter);

    //Task<IEnumerable<ProductDto>?> GetByCategoryAsync(string categoryId, ProductFilterRequestDto filter);
    Task<ProductFilterResponseDto?> GetBySellerIdAsync(string sellerId, ProductFilterRequestDto filter);
    Task<ProductDto> CreateAsync(ProductCreateDto productDto);
    Task<ProductDto?> UpdateAsync(ProductDto productDto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<ProductSearchResultDto>?> SearchByNameAsync(string name);
}