using App.Core.Models.Product;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace App.Core.Interfaces;

public interface IProductService
{
    Task<Product?> GetByIdAsync(string id);
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> SearchAsync(string? name = null, string? category = null, string? sellerId = null);
    Task CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(string id);

    // Variation management
    Task<bool> AddVariationAsync(string productId, ProductVariation variation);
    Task<bool> UpdateVariationAsync(string productId, ProductVariation variation);
    Task<bool> RemoveVariationAsync(string productId, string modelName);

    // Media management
    Task<bool> AddMediaAsync(string productId, string modelName, ProductMedia media);
    Task<bool> RemoveMediaAsync(string productId, string modelName, string mediaId);
} 