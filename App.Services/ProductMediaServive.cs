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
        List<ProductMediaDto> dtos = [];
        if (media == null || media.Count == 0) return [];
        foreach (var m in media)
        {
            var dto = new ProductMediaDto
            {
                Id = m.Id.ToString(),
                ProductId = m.ProductId.ToString(),
                UrlFileName = m.UrlFileName,
                Url = m.Url,
                Type = m.Type,
                Order = m.Order,
                SecondaryUrl = m.SecondaryUrl,
                SecondUrlFileName = m.SecondUrlFileName
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
        string urlFileName = "";
        string? secondUrlFileName = null;

        if (type == MediaType.Image)
        {
            var (fullHdPath, hdPath, _urlFileName, _secondUrlFileName) = await _fileService.SaveImageAsync(stream, fileName, _productMediaKeys.Image);
            url = fullHdPath;
            secondaryUrl = hdPath;
            urlFileName = _urlFileName;
            secondUrlFileName = _secondUrlFileName;
        }
        else if (type == MediaType.Video)
        {
            var (_url, _fileName) = await _fileService.SaveVideoAsync(stream, fileName, _productMediaKeys.Video);
            url = _url;
            urlFileName = _fileName;
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
            urlFileName,
            url,
            type,
            order,
            secondaryUrl,
            secondUrlFileName
        );

        await _repository.SaveAsync(media);

        var dto = new ProductMediaDto
        {
            Id = media.Id.ToString(),
            ProductId = media.ProductId.ToString(),
            UrlFileName = media.UrlFileName,
            Url = media.Url,
            Type = media.Type,
            Order = media.Order,
            SecondaryUrl = media.SecondaryUrl,
            SecondUrlFileName = media.SecondUrlFileName
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
                if (media.Type == MediaType.Image)
                {
                    //Console.WriteLine("Delete image: ");
                    //Console.WriteLine(media.UrlFileName);
                    await _fileService.DeleteFileAsync(_productMediaKeys.Image, media.UrlFileName);
                    if (media.SecondaryUrl != null && media.SecondUrlFileName != null) await _fileService.DeleteFileAsync(_productMediaKeys.Image, media.SecondUrlFileName);
                }
                else if (media.Type == MediaType.Video)
                {
                    //Console.WriteLine("Delete video: ");
                    //Console.WriteLine(media.UrlFileName);
                    await _fileService.DeleteFileAsync(_productMediaKeys.Video, media.UrlFileName);
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

        await _fileService.DeleteFileAsync(_productMediaKeys.Image , media.Url);
        if (media.SecondaryUrl != null) await _fileService.DeleteFileAsync(_productMediaKeys.Image, media.SecondaryUrl);
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
                await _fileService.DeleteFileAsync(_productMediaKeys.Image, m.UrlFileName);
                if (m.SecondaryUrl != null && m.SecondUrlFileName != null) await _fileService.DeleteFileAsync(_productMediaKeys.Image, m.SecondUrlFileName);
            }
            else if (m.Type == MediaType.Video)
            {
                await _fileService.DeleteFileAsync(_productMediaKeys.Video, m.UrlFileName);
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

    /// <summary>
    /// Trims the "wwwroot/" prefix from a file path for frontend-friendly URLs.
    /// </summary>
    /// <param name="path">Path to trim.</param>
    /// <returns>Trimmed path or original if prefix not present.</returns>
    // private string? TrimRoot(string? path) =>
    //         !string.IsNullOrWhiteSpace(path) && path.StartsWith(RootPrefix)
    //             ? path.Substring(RootPrefix.Length)
    //             : path;
}
