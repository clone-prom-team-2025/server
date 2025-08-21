using System.Runtime.InteropServices;
using App.Core.Models.Product;

namespace App.Core.Interfaces;

public interface IProductMediaRepository
{
    Task<List<ProductMedia>?> GetAll();
    Task SaveAsync(ProductMedia media);
    Task<bool> UpdateAsync(ProductMedia media);
    Task<ProductMedia?> GetByIdAsync(string id);
    // Task<ProductMedia?> GetFilenameAsync(string fileName);
    Task<List<ProductMedia>?> GetByProductIdAsync(string productId);
    Task<bool> RemoveAsync(string id);
    Task<bool> RemoveByProdutIdAsync(string productId);
}