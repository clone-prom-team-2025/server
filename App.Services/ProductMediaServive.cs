using App.Core.DTOs.Product;
using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.Product;
using App.Core.Models;
using AutoMapper;
using MongoDB.Bson;
using App.Core.DTOs;
using App.Core.Models.FileStorage;
using Microsoft.Extensions.Options;

namespace App.Services;

/// <summary>
/// Service responsible for handling operations related to product media (images and videos).
/// Provides functionality to save, update, retrieve and delete media files associated with products.
/// </summary>
public class ProductMediaService(IProductMediaRepository repository, IFileService fileService, IMapper mapper, IOptions<ProductMediaKeys> productMediaKeys) : IProductMediaService
{
    /// <summary>
    /// Repository interface for accessing product media data from the database.
    /// </summary>
    private readonly IProductMediaRepository _repository = repository;

    private readonly ProductMediaKeys _productMediaKeys = productMediaKeys.Value;

    /// <summary>
    /// Service responsible for saving and deleting files on disk.
    /// </summary>
    private readonly IFileService _fileService = fileService;

    /// <summary>
    /// AutoMapper instance for converting between entities and DTOs.
    /// </summary>
    private readonly IMapper _mapper = mapper;

    //private const string RootPrefix = "wwwroot/";

    /// <summary>
    /// Retrieves all product media from the database.
    /// </summary>
    /// <returns>List of <see cref="ProductMediaDto"/> or null if none found.</returns>
    public async Task<List<ProductMediaDto>?> GetAll()
    {
        var media = await _repository.GetAll();
        if (media == null || media.Count == 0) return [];
        return _mapper.Map<List<ProductMediaDto>>(media);
    }

    /// <summary>
    /// Saves a new product media file (image or video) to disk and stores its metadata in the database.
    /// </summary>
    /// <param name="productId">ID of the associated product.</param>
    /// <param name="stream">Stream of the uploaded file.</param>
    /// <param name="fileName">Name of the uploaded file.</param>
    /// <param name="order">Ordering position of the media among other media for the same product.</param>
    /// <returns>The saved <see cref="ProductMediaDto"/>.</returns>
    /// <exception cref="InvalidOperationException">If file is not safe.</exception>
    /// <exception cref="ArgumentException">If media type is unsupported.</exception>
    public async Task<ProductMediaDto> PushMediaAsync(string productId, Stream stream, string fileName, int order)
    {
        if (!MediaInspector.IsSafeMedia(stream, fileName))
            throw new InvalidOperationException("Invalid or potentially harmful file");

        var type = MediaInspector.GetMediaType(stream, fileName);

        BaseFile file = new();

        if (type == MediaType.Image)
        {
            (file.SourceUrl, file.CompressedUrl, file.SourceFileName, file.CompressedFileName) = await _fileService.SaveImageAsync(stream, fileName, _productMediaKeys.Image);
        }
        else if (type == MediaType.Video)
        {
            (file.SourceUrl, file.SourceFileName) = await _fileService.SaveVideoAsync(stream, fileName, _productMediaKeys.Video);
        }
        else
        {
            throw new ArgumentException("Unsupported media type");
        }

        var medias = await _repository.GetByProductIdAsync(productId);
        if (medias == null || medias.Count == 0) order = 0;
        else
        {
            medias = medias.OrderByDescending(m => m.Order).ToList();
            order = medias.First().Order + 1;
        }

        var media = new ProductMedia(
            ObjectId.Parse(productId),
            type,
            order,
            file
        );

        await _repository.SaveAsync(media);

        stream.Close();
        return _mapper.Map<ProductMediaDto>(media);
    }

    /// <summary>
    /// Replaces all media of the specified product with new files from temporary paths.
    /// </summary>
    /// <param name="files">List of media files with temporary URLs.</param>
    /// <param name="productId">The ID of the product.</param>
    /// <returns>List of newly added media DTOs.</returns>
    public async Task<List<ProductMediaDto>?> SyncMediaFromTempFilesAsync(List<FileArrayItemDto> files, string productId)
    {
        var existing = await _repository.GetByProductIdAsync(productId);
        if (existing is { Count: > 0 })
        {
            foreach (var media in existing)
            {
                await _repository.RemoveAsync(media.Id.ToString());
                if (media.Type == MediaType.Image)
                {
                    await _fileService.DeleteFileAsync(_productMediaKeys.Image, media.Files.SourceFileName);
                    if (media.Files.CompressedUrl != null && media.Files.CompressedFileName != null) await _fileService.DeleteFileAsync(_productMediaKeys.Image, media.Files.CompressedFileName);
                }
                else if (media.Type == MediaType.Video)
                {
                    await _fileService.DeleteFileAsync(_productMediaKeys.Video, media.Files.SourceFileName);
                }
            }
        }

        List<ProductMediaDto> result = [];

        for (int i = 0; i < files.Count; i++)
        {
            var path = files[i].Url!;
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            result.Add(await PushMediaAsync(productId, stream, files[i].FileName, i));
        }

        return result;
    }

    /// <summary>
    /// Deletes a media item by its ID.
    /// Also removes associated files from disk.
    /// </summary>
    /// <param name="id">ID of the media to delete.</param>
    /// <returns>True if deletion was successful; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(string id)
    {
        var media = await _repository.GetByIdAsync(id);
        if (media == null) return false;

        await _fileService.DeleteFileAsync(_productMediaKeys.Image , media.Files.SourceFileName);
        if (media.Files.CompressedUrl != null && media.Files.CompressedFileName != null) await _fileService.DeleteFileAsync(_productMediaKeys.Image, media.Files.CompressedFileName);
        return await _repository.RemoveAsync(id);
    }

    /// <summary>
    /// Deletes a media item by product ID.
    /// Also removes associated files from disk.
    /// </summary>
    /// <param name="productId">ID of the product to delete its media.</param>
    /// <returns>True if deletion was successful; otherwise, false.</returns>
    public async Task<bool> DeleteByProductIdAsync(string productId)
    {
        var media = await _repository.GetByProductIdAsync(productId);
        if (media == null) return false;
        if (media.Count == 0) return false;

        foreach (var m in media)
        {
            if (m.Type == MediaType.Image)
            {
                await _fileService.DeleteFileAsync(_productMediaKeys.Image, m.Files.SourceFileName);
                if (m.Files.CompressedUrl != null && m.Files.CompressedFileName != null) await _fileService.DeleteFileAsync(_productMediaKeys.Image, m.Files.CompressedFileName);
            }
            else if (m.Type == MediaType.Video)
            {
                await _fileService.DeleteFileAsync(_productMediaKeys.Video, m.Files.SourceFileName);
            }
        }
        
        return await _repository.RemoveByProdutIdAsync(productId);;
    }

    /// <summary>
    /// Gets all media items associated with a given product.
    /// </summary>
    /// <param name="productId">ID of the product.</param>
    /// <returns>Collection of <see cref="ProductMediaDto"/> or null if none found.</returns>
    public async Task<List<ProductMediaDto>?> GetByProductIdAsync(string productId)
    {
        var list = await _repository.GetByProductIdAsync(productId);
        return _mapper.Map<List<ProductMediaDto>?>(list);
    }
}
