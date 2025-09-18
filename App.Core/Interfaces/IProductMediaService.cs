using App.Core.DTOs;
using App.Core.DTOs.Product;

namespace App.Services;

public interface IProductMediaService
{
    Task<List<ProductMediaDto>?> GetAll();
    Task<ProductMediaDto> PushMediaAsync(string productId, Stream stream, string fileName, int order);
    Task<List<ProductMediaDto>?> SyncMediaFromTempFilesAsync(List<FileArrayItemDto> files, string productId, string userId);
    Task DeleteAsync(string id);
    Task<List<ProductMediaDto>?> GetByProductIdAsync(string productId);
    Task DeleteByProductIdAsync(string productId);
}