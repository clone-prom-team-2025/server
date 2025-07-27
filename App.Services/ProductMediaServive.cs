using App.Core.DTOs.Product;
using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.Product;
using App.Core.Models;
using AutoMapper;
using MongoDB.Bson;
using App.Core.DTOs;

namespace App.Services;

/// <summary>
/// Service responsible for handling operations related to product media (images and videos).
/// Provides functionality to save, update, retrieve and delete media files associated with products.
/// </summary>
public class ProductMediaService(IProductMediaRepository repository, IFileService fileService, IMapper mapper) : IProductMediaService
{
    /// <summary>
    /// Repository interface for accessing product media data from the database.
    /// </summary>
    private readonly IProductMediaRepository _repository = repository;

    /// <summary>
    /// Service responsible for saving and deleting files on disk.
    /// </summary>
    private readonly IFileService _fileService = fileService;

    /// <summary>
    /// AutoMapper instance for converting between entities and DTOs.
    /// </summary>
    private readonly IMapper _mapper = mapper;

    private const string RootPrefix = "wwwroot/";

    /// <summary>
    /// Retrieves all product media from the database.
    /// </summary>
    /// <returns>List of <see cref="ProductMediaDto"/> or null if none found.</returns>
    public async Task<List<ProductMediaDto>?> GetAll()
    {
        var media = await _repository.GetAll();
        List<ProductMediaDto> dtos = [];
        if (media == null || media.Count == 0) return [];
        foreach (var m in media)
        {
            var dto = new ProductMediaDto
            {
                Id = m.Id.ToString(),
                ProductId = m.ProductId.ToString(),
                FileName = m.FileName,
                Url = TrimRoot(m.Url),
                Type = m.Type,
                Order = m.Order,
                SecondaryUrl = TrimRoot(m.SecondaryUrl)
            };
            dtos.Add(dto);
        }

        return dtos;
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

        string url;
        string? secondaryUrl = null;
        string savedFileName = "";

        if (type == MediaType.Image)
        {
            var (fullHdPath, hdPath, _savedFileName) = await _fileService.SaveImageAsync(stream, fileName);
            url = fullHdPath;
            secondaryUrl = hdPath;
            savedFileName = _savedFileName;
        }
        else if (type == MediaType.Video)
        {
            var (_url, _fileName) = await _fileService.SaveVideoAsync(stream, fileName);
            url = _url;
            savedFileName = _fileName;
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
            savedFileName,
            url,
            type,
            order,
            secondaryUrl
        );

        await _repository.SaveAsync(media);

        var dto = new ProductMediaDto
        {
            Id = media.Id.ToString(),
            ProductId = media.ProductId.ToString(),
            FileName = media.FileName,
            Url = TrimRoot(media.Url),
            Type = media.Type,
            Order = media.Order,
            SecondaryUrl = TrimRoot(media.SecondaryUrl)
        };

        stream.Close();
        return dto;
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
                _fileService.DeleteFile(media.Url);
                if (media.SecondaryUrl != null) _fileService.DeleteFile(media.SecondaryUrl);
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
    /// Updates existing media data in the database.
    /// </summary>
    /// <param name="dto">The DTO containing updated media information.</param>
    /// <returns>True if update was successful; otherwise, false.</returns>
    public async Task<bool> UpdateAsync(ProductMediaDto dto)
    {
        var media = new ProductMedia(ObjectId.Parse(dto.ProductId), dto.FileName, dto.Url, dto.Type, dto.Order)
        {
            Id = ObjectId.Parse(dto.Id)
        };
        return await _repository.UpdateAsync(media);
    }

    /// <summary>
    /// Performs upsert operation (update if exists, insert if not) for a collection of media DTOs.
    /// </summary>
    /// <param name="mediaDtos">Collection of <see cref="ProductMediaDto"/> objects.</param>
    /// <returns>True if all operations succeeded; otherwise, false.</returns>
    public async Task<bool> UpsertManyAsync(List<ProductMediaDto> mediaDtos)
    {
        bool result = true;

        foreach (var dto in mediaDtos)
        {
            var exists = await _repository.GetByIdAsync(dto.Id);
            if (exists != null)
            {
                result &= await UpdateAsync(dto);
            }
            else
            {
                var media = new ProductMedia(ObjectId.Parse(dto.ProductId), dto.FileName, dto.Url, dto.Type, dto.Order)
                {
                    Id = ObjectId.Parse(dto.Id)
                };
                await _repository.SaveAsync(media);
            }
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

        _fileService.DeleteFile(media.Url);
        if (media.SecondaryUrl != null) _fileService.DeleteFile(media.SecondaryUrl);
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
            _fileService.DeleteFile(m.Url);
            if (m.SecondaryUrl != null) _fileService.DeleteFile(m.SecondaryUrl);
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

    /// <summary>
    /// Deletes all media entries related to a specific product.
    /// Also deletes their files from disk.
    /// </summary>
    /// <param name="produtId">ID of the product (typo in parameter name preserved from original).</param>
    /// <returns>True if deletion was successful; otherwise, false.</returns>
    public async Task<bool> DeleteByProductId(string produtId)
    {
        var media = await _repository.GetByProductIdAsync(produtId);
        if (media == null || media.Count == 0) return false;

        foreach (var m in media)
        {
            if (m != null) _fileService.DeleteFile(m.Url);
            if (m != null && m.SecondaryUrl != null) _fileService.DeleteFile(m.SecondaryUrl);
        }
        return await _repository.RemoveByProdutIdAsync(produtId);
    }

    /// <summary>
    /// Trims the "wwwroot/" prefix from a file path for frontend-friendly URLs.
    /// </summary>
    /// <param name="path">Path to trim.</param>
    /// <returns>Trimmed path or original if prefix not present.</returns>
    private string? TrimRoot(string? path) =>
            !string.IsNullOrWhiteSpace(path) && path.StartsWith(RootPrefix)
                ? path.Substring(RootPrefix.Length)
                : path;
}
