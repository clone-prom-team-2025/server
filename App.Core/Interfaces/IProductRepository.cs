using App.Core.Models.Product;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>?> GetAllAsync(ProductFilterRequest filter);
    Task<Product?> GetByIdAsync(ObjectId id);
    Task<IEnumerable<Product>?> GetByNameAsync(string name, ProductFilterRequest filter);
    //Task<IEnumerable<Product>?> GetByCategoryAsync(ObjectId categoryId, ProductFilterRequest filter);
    Task<IEnumerable<Product>?> GetBySellerIdAsync(ObjectId sellerId, ProductFilterRequest filter);
    Task<Product> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(ObjectId id);
    Task<List<ProductSearchResult>> SearchByNameAsync(string name);
}