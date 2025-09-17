using App.Core.Models.Product;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IProductRepository
{
    Task<ProductFilterResponse?> GetAllAsync(ProductFilterRequest filter);
    Task<Product?> GetByIdAsync(ObjectId id);

    Task<ProductFilterResponse?> GetByNameAsync(string name, ProductFilterRequest filter);

    Task<ProductFilterResponse?> GetBySellerIdAsync(ObjectId sellerId, ProductFilterRequest filter);
    Task<Product> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(ObjectId id);
    Task<List<ProductSearchResult>> SearchByNameAsync(string name);
    
    Task<bool> ExistById(ObjectId id);
    Task<bool> ExistBySellerId(ObjectId sellerId);
}