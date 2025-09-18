using App.Core.DTOs.Product;

namespace App.Core.Interfaces;

public interface IProductService
{
    Task<ProductFilterResponseDto?> GetAllAsync(ProductFilterRequestDto filter);
    Task<ProductDto?> GetByIdAsync(string id);

    Task<ProductFilterResponseDto?> GetByNameAsync(string name, ProductFilterRequestDto filter);

    //Task<IEnumerable<ProductDto>?> GetByCategoryAsync(string categoryId, ProductFilterRequestDto filter);
    Task<ProductFilterResponseDto?> GetBySellerIdAsync(string sellerId, ProductFilterRequestDto filter);
    Task CreateAsync(ProductCreateDto productDto, string userId);
    Task UpdateAsync(UpdateProductDto productDto, string userId);
    Task DeleteAsync(string id, string userId);
    Task<IEnumerable<ProductSearchResultDto>?> SearchByNameAsync(string name);
    Task<IEnumerable<ProductDto>> GetRandomProductsAsync(int page, int pageSize);
}