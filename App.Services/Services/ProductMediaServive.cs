using App.Core.DTOs;
using App.Core.DTOs.Product;
using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.FileStorage;
using App.Core.Models.Product;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace App.Services.Services;

/// <summary>
///     Service responsible for handling operations related to product media (images and videos).
///     Provides functionality to save, update, retrieve and delete media files associated with products.
/// </summary>
public class ProductMediaService(
    IProductMediaRepository repository,
    IFileService fileService,
    IMapper mapper,
    IOptions<ProductMediaKeys> productMediaKeys,
    ILogger<ProductMediaService> logger) : IProductMediaService
{
    /// <summary>
    ///     Service responsible for saving and deleting files on disk.
    /// </summary>
    private readonly IFileService _fileService = fileService;

    /// <summary>
    ///     AutoMapper instance for converting between entities and DTOs.
    /// </summary>
    private readonly IMapper _mapper = mapper;

    private readonly ProductMediaKeys _productMediaKeys = productMediaKeys.Value;

    /// <summary>
    ///     Repository interface for accessing product media data from the database.
    /// </summary>
    private readonly IProductMediaRepository _repository = repository;

    private readonly ILogger<ProductMediaService> _logger = logger;

    /// <summary>
    ///     Retrieves all product media from the database.
    /// </summary>
    /// <returns>List of <see cref="ProductMediaDto" /> or null if none found.</returns>
    public async Task<List<ProductMediaDto>?> GetAll()
    {
        using (_logger.BeginScope("GetAll")){
            _logger.LogInformation("GetAll called");
            var media = await _repository.GetAll();
            if (media == null || media.Count == 0) return [];
            _logger.LogInformation("GetAll success");
            return _mapper.Map<List<ProductMediaDto>>(media);
        }
    }

    /// <summary>
    ///     Saves a new product media file (image or video) to disk and stores its metadata in the database.
    /// </summary>
    /// <param name="productId">ID of the associated product.</param>
    /// <param name="stream">Stream of the uploaded file.</param>
    /// <param name="fileName">Name of the uploaded file.</param>
    /// <param name="order">Ordering position of the media among other media for the same product.</param>
    public async Task<ProductMediaDto> PushMediaAsync(string productId, Stream stream, string fileName, int order)
    {
        using (_logger.BeginScope("PushMediaAsync")){
            _logger.LogInformation("PushMediaAsync called");
            // if (!MediaInspector.IsSafeMedia(stream, fileName))
            //     throw new InvalidOperationException("Invalid or potentially harmful file");

            var type = MediaInspector.GetMediaType(stream, fileName);

            BaseFile file = new();

            if (type == MediaType.Image)
                (file.SourceUrl, file.CompressedUrl, file.SourceFileName, file.CompressedFileName) =
                    await _fileService.SaveImageAsync(stream, fileName, _productMediaKeys.Image);
            else if (type == MediaType.Video)
                (file.SourceUrl, file.SourceFileName) =
                    await _fileService.SaveVideoAsync(stream, fileName, _productMediaKeys.Video);
            else
                throw new InvalidOperationException("Unsupported media type");

            var medias = await _repository.GetByProductIdAsync(productId);
            if (medias == null || medias.Count == 0)
            {
                order = 0;
            }
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
            _logger.LogInformation("PushMediaAsync success");
            return _mapper.Map<ProductMediaDto>(media);
        }
    }

    /// <summary>
    ///     Replaces all media of the specified product with new files from temporary paths.
    /// </summary>
    /// <param name="files">List of media files with temporary URLs.</param>
    /// <param name="productId">The ID of the product.</param>
    /// <returns>List of newly added media DTOs.</returns>
    public async Task<List<ProductMediaDto>?> SyncMediaFromTempFilesAsync(List<FileArrayItemDto> files,
        string productId)
    {
        using (_logger.BeginScope("SyncMediaFromTempFilesAsync")){
            _logger.LogInformation("SyncMediaFromTempFilesAsync called");
            // foreach (var file in files)
            //     if (!MediaInspector.IsSafeMedia(file.Stream, file.FileName))
            //         throw new InvalidOperationException("Invalid or potentially harmful file");

            var existing = await _repository.GetByProductIdAsync(productId);
            if (existing is { Count: > 0 })
                foreach (var media in existing)
                {
                    await _repository.RemoveAsync(media.Id.ToString());
                    if (media.Type == MediaType.Image)
                    {
                        await _fileService.DeleteFileAsync(_productMediaKeys.Image, media.Files.SourceFileName);
                        if (media.Files.CompressedUrl != null && media.Files.CompressedFileName != null)
                            await _fileService.DeleteFileAsync(_productMediaKeys.Image, media.Files.CompressedFileName);
                    }
                    else if (media.Type == MediaType.Video)
                    {
                        await _fileService.DeleteFileAsync(_productMediaKeys.Video, media.Files.SourceFileName);
                    }
                }

            List<ProductMediaDto> result = [];

            for (var i = 0; i < files.Count; i++)
            {
                var path = files[i].Url!;
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                result.Add(await PushMediaAsync(productId, stream, files[i].FileName, i));
            }

            _logger.LogInformation("SyncMediaFromTempFilesAsync success");
            return result;
        }
    }

    /// <summary>
    ///     Deletes a media item by its ID.
    ///     Also removes associated files from disk.
    /// </summary>
    /// <param name="id">ID of the media to delete.</param>
    /// <returns>True if deletion was successful; otherwise, false.</returns>
    public async Task DeleteAsync(string id)
    {
        using (_logger.BeginScope("DeleteAsync")) {
            _logger.LogInformation("DeleteAsync called");
            var media = await _repository.GetByIdAsync(id);
            if (media == null) throw new KeyNotFoundException("Media not found");

            await _fileService.DeleteFileAsync(_productMediaKeys.Image, media.Files.SourceFileName);
            if (media.Files.CompressedUrl != null && media.Files.CompressedFileName != null)
                await _fileService.DeleteFileAsync(_productMediaKeys.Image, media.Files.CompressedFileName);
            if (!await _repository.RemoveAsync(id))
                throw new InvalidOperationException("Can't delete media");
            _logger.LogInformation("DeleteAsync success");
        }
    }

    /// <summary>
    ///     Deletes a media item by product ID.
    ///     Also removes associated files from disk.
    /// </summary>
    /// <param name="productId">ID of the product to delete its media.</param>
    /// <returns>True if deletion was successful; otherwise, false.</returns>
    public async Task DeleteByProductIdAsync(string productId)
    {
        using (_logger.BeginScope("DeleteByProductIdAsync")) {
            _logger.LogInformation("DeleteByProductIdAsync called");
            var media = await _repository.GetByProductIdAsync(productId);
            if (media == null) return;
            if (media.Count == 0) return;

            foreach (var m in media)
                if (m.Type == MediaType.Image)
                {
                    await _fileService.DeleteFileAsync(_productMediaKeys.Image, m.Files.SourceFileName);
                    if (m.Files.CompressedUrl != null && m.Files.CompressedFileName != null)
                        await _fileService.DeleteFileAsync(_productMediaKeys.Image, m.Files.CompressedFileName);
                }
                else if (m.Type == MediaType.Video)
                {
                    await _fileService.DeleteFileAsync(_productMediaKeys.Video, m.Files.SourceFileName);
                }

            if (!await _repository.RemoveByProdutIdAsync(productId))
                throw new InvalidOperationException("Can't delete media");
            _logger.LogInformation("DeleteByProductIdAsync success");
        }
    }

    /// <summary>
    ///     Gets all media items associated with a given product.
    /// </summary>
    /// <param name="productId">ID of the product.</param>
    /// <returns>Collection of <see cref="ProductMediaDto" /> or null if none found.</returns>
    public async Task<List<ProductMediaDto>?> GetByProductIdAsync(string productId)
    {
        using (_logger.BeginScope("GetByProductIdAsync")){
            _logger.LogInformation("GetByProductIdAsync called");
            var list = await _repository.GetByProductIdAsync(productId);
            _logger.LogInformation("GetByProductIdAsync success");
            return _mapper.Map<List<ProductMediaDto>?>(list);
        }
    }
}