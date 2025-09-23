using App.Core.Archive.Product;
using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.Product;
using App.Core.Models.Product.Review;
using App.Data;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Services;

public class ArchiveAndCleanupManager : IArchiveAndCleanupManager
{
    // Other
    private readonly ILogger<ArchiveAndCleanupManager> _logger;
    private readonly IMapper _mapper;
    private readonly IMongoClient _client;
    private readonly IFileService _fileService;
    private static readonly TimeSpan ArchiveLifetime = TimeSpan.FromDays(30);
    
    // Products
    private readonly IMongoCollection<Product> _productCollection;
    private readonly IMongoCollection<ProductMedia> _productMediaCollection;
    private readonly IMongoCollection<ProductReview> _productReviewCollection;
    private readonly IMongoCollection<ProductArchive> _productArchiveCollection;
    private readonly IMongoCollection<ProductMediaArchive> _productMediaArchiveCollection;
    private readonly IMongoCollection<ProductReviewArchive> _productReviewArchiveCollection;
    
    public ArchiveAndCleanupManager(MongoDbContext context, ILogger<ArchiveAndCleanupManager> logger, IMapper mapper, IMongoClient client, IFileService fileService)
    {
        _logger = logger;
        _mapper = mapper;
        _client = client;
        
        _productCollection = context.Products;
        _productMediaCollection = context.ProductMedia;
        _productReviewCollection = context.ProductReviews;
        _productArchiveCollection = context.ProductArchives;
        _productMediaArchiveCollection = context.ProductMediaArchives;
        _productReviewArchiveCollection = context.ProductReviewArchives;
    }
    
    #region Products
    public async Task SoftDeleteProductAsync(ObjectId id)
    {
        using var scope = _logger.BeginScope("Deleting Product: {id}", id);
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            var productFilter = Builders<Product>.Filter.Eq(d => d.Id, id);
            var product = await _productCollection.Find(productFilter).FirstOrDefaultAsync();
            if (product == null)
            {
                _logger.LogError("Cleaner cannot find Product by Id {id}", id);
                throw new KeyNotFoundException("Product not found");
            }

            // --- Product ---
            var archivedProduct = _mapper.Map<ProductArchive>(product);
            archivedProduct.Id = $"archived_{id}";
            archivedProduct.ArchivedAt = DateTime.UtcNow;
            await _productArchiveCollection.InsertOneAsync(session, archivedProduct);
            await _productCollection.DeleteManyAsync(session, productFilter);
            _logger.LogInformation($"Archived {nameof(Product)}");

            // --- ProductMedia ---
            var productMediaFilter = Builders<ProductMedia>.Filter.Eq(d => d.ProductId, id);
            var productMediaList = await _productMediaCollection.Find(productMediaFilter).ToListAsync();
            if (productMediaList.Count > 0)
            {
                var archivedMedia = _mapper.Map<List<ProductMediaArchive>>(productMediaList)
                                         .Select(m => { m.ArchivedAt = DateTime.UtcNow; m.Id = $"archived_{m.Id}"; return m; });
                await _productMediaArchiveCollection.InsertManyAsync(session, archivedMedia);
                await _productMediaCollection.DeleteManyAsync(session, productMediaFilter);
                _logger.LogInformation($"Archived {nameof(ProductMedia)}");
            }
            else
            {
                _logger.LogInformation($"No {nameof(ProductMedia)} to archive");
            }

            // --- ProductReview ---
            var productReviewFilter = Builders<ProductReview>.Filter.Eq(d => d.ProductId, id);
            var productReviewList = await _productReviewCollection.Find(productReviewFilter).ToListAsync();
            if (productReviewList.Count > 0)
            {
                var archivedReviews = _mapper.Map<List<ProductReviewArchive>>(productReviewList)
                                            .Select(r => { r.ArchivedAt = DateTime.UtcNow; r.Id = $"archived_{r.Id}"; return r; });
                await _productReviewArchiveCollection.InsertManyAsync(session, archivedReviews);
                await _productReviewCollection.DeleteManyAsync(session, productReviewFilter);
                _logger.LogInformation($"Archived {nameof(ProductReview)}");
            }
            else
            {
                _logger.LogInformation($"No {nameof(ProductReview)} to archive");
            }

            await session.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            _logger.LogError(ex, "Error while archiving product with all related data {id}", id);
            throw;
        }
    }

    public async Task PermanentlyDeleteProductAsync(ObjectId id)
    {
        using var scope = _logger.BeginScope("Deleting Product: {id}", id);
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            // --- Product ---
            var productFilter = Builders<ProductArchive>.Filter.Eq(d => d.ProductId, id);
            var product = await _productArchiveCollection.Find(productFilter).FirstOrDefaultAsync();
            if (product == null)
            {
                _logger.LogWarning("No archived product found with Id {id}", id);
            }

            // --- ProductMedia ---
            var productMediaFilter = Builders<ProductMediaArchive>.Filter.Eq(d => d.ProductId, id);
            var productMediaList = await _productMediaArchiveCollection.Find(productMediaFilter).ToListAsync();

            // --- ProductReview ---
            var productReviewFilter = Builders<ProductReviewArchive>.Filter.Eq(d => d.ProductId, id);
            var productReviewList = await _productReviewArchiveCollection.Find(productReviewFilter).ToListAsync();

            await _productMediaArchiveCollection.DeleteManyAsync(session, productMediaFilter);
            await _productArchiveCollection.DeleteManyAsync(session, productFilter);
            await _productReviewArchiveCollection.DeleteManyAsync(session, productReviewFilter);

            await session.CommitTransactionAsync();

            _logger.LogInformation($"Permanently removed {nameof(Product)}, {nameof(ProductMedia)} and {nameof(ProductReview)} for ProductId {id}");

            foreach (var media in productMediaList)
            {
                try
                {
                    if (media.Type == MediaType.Image)
                    {
                        await _fileService.DeleteFileAsync("products-images", media.Files.SourceFileName);
                        if (!string.IsNullOrEmpty(media.Files.CompressedFileName))
                            await _fileService.DeleteFileAsync("products-images", media.Files.CompressedFileName);
                    }
                    else if (media.Type == MediaType.Video)
                    {
                        await _fileService.DeleteFileAsync("products-videos", media.Files.SourceFileName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete media file {fileName}", media.Files.SourceFileName);
                }
            }
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            _logger.LogError(ex, "Error while permanently deleting product {id}", id);
            throw;
        }
    }
    
    public async Task RestoreProductAsync(ObjectId id)
    {
        using var scope = _logger.BeginScope("Restore Product: {id}", id);
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();
        
        var archivedProductFilter = Builders<ProductArchive>.Filter.Eq(d => d.ProductId, id);
        var archivedProductCollection = await _productArchiveCollection.Find(archivedProductFilter).FirstOrDefaultAsync();
        
        var archivedProductMediaFilter = Builders<ProductMediaArchive>.Filter.Eq(d => d.ProductId, id);
        var archivedProductMediaCollection = await _productMediaArchiveCollection.Find(archivedProductMediaFilter).ToListAsync();
        
        var archivedProductReviewFilter = Builders<ProductReviewArchive>.Filter.Eq(d => d.ProductId, id);
        var archivedProductReviewCollection = await _productReviewArchiveCollection.Find(archivedProductReviewFilter).ToListAsync();

        if (archivedProductCollection == null)
        {
            _logger.LogWarning("No archived product found with Id {id}", id.ToString());
            throw new KeyNotFoundException($"No archived product found with Id {id.ToString()}");
        }
        
        var productCollection = _mapper.Map<Product>(archivedProductCollection);
        var productMediaCollection = _mapper.Map<List<ProductMedia>>(archivedProductMediaCollection);
        var productReviewCollection = _mapper.Map<List<ProductReview>>(archivedProductReviewCollection);
        
        try
        {
            await _productCollection.InsertOneAsync(productCollection);
            if (productMediaCollection.Count > 0)
                await _productMediaCollection.InsertManyAsync(productMediaCollection);
            if (productReviewCollection.Count > 0)
                await _productReviewCollection.InsertManyAsync(productReviewCollection);

            await _productArchiveCollection.DeleteManyAsync(session, archivedProductFilter);
            if (productMediaCollection.Count > 0)
                await _productMediaArchiveCollection.DeleteManyAsync(session, Builders<ProductMediaArchive>.Filter.Eq(d => d.ProductId, id));
            if (productReviewCollection.Count > 0)
                await _productReviewArchiveCollection.DeleteManyAsync(session, Builders<ProductReviewArchive>.Filter.Eq(d => d.ProductId, id));

            await session.CommitTransactionAsync();
        }
        catch (Exception e)
        {
            await session.AbortTransactionAsync();
            _logger.LogError(e, "Error while restoring product");
            throw;
        }
        _logger.LogInformation($"Restored {nameof(Product)} {id.ToString()}");
    }

    public async Task CleanupOldArchivedProductsAsync()
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-ArchiveLifetime.TotalDays);
        var oldProducts = await _productArchiveCollection
            .Find(p => p.ArchivedAt <= thresholdDate)
            .ToListAsync();

        foreach (var p in oldProducts)
        {
            await PermanentlyDeleteProductAsync(p.ProductId);
        }
    }

    public async Task<ProductArchive> GetArchivedProductsAsync(ObjectId id)
    {
        using var scope = _logger.BeginScope("Get Archived Products");
        var archivedProductCollection = await _productArchiveCollection.Find(p => p.ProductId == id).FirstOrDefaultAsync();
        return archivedProductCollection;
    }

    public async Task<IEnumerable<ProductArchive>> GetProductArchiveCollection(ObjectId sellerId)
    {
        using var scope = _logger.BeginScope("Get Product Archive Collection");
        var archivedProductCollection = await _productArchiveCollection.Find(p => p.SellerId == sellerId).ToListAsync();
        return archivedProductCollection;
    }
    #endregion
}