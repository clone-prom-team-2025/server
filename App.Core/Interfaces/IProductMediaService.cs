using App.Core.DTOs;
using App.Core.DTOs.Product;
using App.Core.Enums;

namespace App.Services;

public interface IProductMediaService
{
    Task<List<ProductMediaDto>?> GetAll();
    Task<ProductMediaDto> PushMediaAsync(string productId, Stream stream, string fileName, int order);
    Task<List<ProductMediaDto>?> SyncMediaFromTempFilesAsync(List<FileArrayItemDto> files, string productId);
    Task<bool> DeleteAsync(string id);
    Task<List<ProductMediaDto>?> GetByProductIdAsync(string productId);
    Task<bool> DeleteByProductIdAsync(string productId);
}
