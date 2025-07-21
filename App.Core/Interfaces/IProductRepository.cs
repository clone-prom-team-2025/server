using App.Core.Models.Product;

namespace App.Core.Interfaces;

public interface IProductRepository
{
    Task<List<Product>?> GetAllAsync(ProductFilterRequest filter);
    Task<Product?> GetByIdAsync(string id);
    Task<List<Product>?> GetByNameAsync(string name, ProductFilterRequest filter);
    Task<List<Product>?> GetByCategoryAsync(string categoryId, ProductFilterRequest filter);
    Task<List<Product>?> GetBySellerIdAsync(string sellerId, ProductFilterRequest filter);
    Task<Product?> GetByModelIdAsync(string modelId, ProductFilterRequest filter);
    Task<List<Product>?> GetByModelIdsAsync(List<string> modelId, ProductFilterRequest filter);
    Task CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(string id);
    Task<List<ProductSearchResult>?> SearchByNameAsync(string name, string languageCode = "en");
}