using App.Core.DTOs.Product;
using App.Core.Enums;
using App.Core.Exceptions;
using App.Core.Interfaces;
using App.Core.Models.Product;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services.Services;

/// <summary>
///     Service for managing products, their variations, and media.
/// </summary>
public class ProductService(
    IProductRepository productRepository,
    IMapper mapper,
    ICategoryRepository categoryRepository,
    IStoreRepository storeRepository,
    IUserRepository userRepository,
    ILogger<ProductService> logger) : IProductService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IStoreRepository _storeRepository = storeRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILogger<ProductService> _logger = logger;

    /// <summary>
    ///     Retrieves all products matching the specified filter.
    /// </summary>
    /// <param name="filter">Filtering options.</param>
    /// <returns>A list of matching products or null.</returns>
    public async Task<ProductFilterResponseDto?> GetAllAsync(ProductFilterRequestDto filter)
    {
        using (_logger.BeginScope("GetAllAsync")){
            _logger.LogInformation("GetAllAsync called");
            var products = await _productRepository.GetAllAsync(_mapper.Map<ProductFilterRequest>(filter));
            _logger.LogInformation("GetAllAsync success");
            return _mapper.Map<ProductFilterResponseDto?>(products);
        }
    }

    /// <summary>
    ///     Retrieves a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product.</param>
    /// <returns>The matching product or null.</returns>
    public async Task<ProductDto?> GetByIdAsync(string id)
    {
        using (_logger.BeginScope("GetByIdAsync")) {
            _logger.LogInformation("GetByIdAsync called");
            var product = await _productRepository.GetByIdAsync(ObjectId.Parse(id));
            _logger.LogInformation("GetByIdAsync success");
            return _mapper.Map<ProductDto>(product);
        }
    }

    /// <summary>
    ///     Retrieves products by name and optional filter.
    /// </summary>
    /// <param name="name">Product name (partial or full).</param>
    /// <param name="filter">Additional filtering options.</param>
    /// <returns>A list of matching products or null.</returns>
    public async Task<ProductFilterResponseDto?> GetByNameAsync(string name, ProductFilterRequestDto filter)
    {
        using (_logger.BeginScope("GetByNameAsync")) {
            _logger.LogInformation("GetByNameAsync called");
            var products = await _productRepository.GetByNameAsync(name, _mapper.Map<ProductFilterRequest>(filter));
            _logger.LogInformation("GetByNameAsync success");
            return _mapper.Map<ProductFilterResponseDto?>(products);
        }
    }

    /// <summary>
    ///     Retrieves products by seller ID.
    /// </summary>
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="filter">Filtering options.</param>
    /// <returns>A list of products by the seller or null.</returns>
    public async Task<ProductFilterResponseDto?> GetBySellerIdAsync(string sellerId, ProductFilterRequestDto filter)
    {
        using  (_logger.BeginScope("GetBySellerIdAsync")) {
            _logger.LogInformation("GetBySellerIdAsync called");
            var products =
                await _productRepository.GetBySellerIdAsync(ObjectId.Parse(sellerId),
                    _mapper.Map<ProductFilterRequest>(filter));
            _logger.LogInformation("GetBySellerIdAsync success");
            return _mapper.Map<ProductFilterResponseDto?>(products);
        }
    }

    /// <summary>
    ///     Creates a new product.
    /// </summary>
    /// <param name="productDto">Data to create the product.</param>
    /// <param name="userId">Product creator.</param>
    /// <returns>The created product DTO.</returns>
    public async Task CreateAsync(ProductCreateDto productDto, string userId)
    {
        using (_logger.BeginScope("CreateAsync")){
            _logger.LogInformation("CreateAsync called");
            var store = await _storeRepository.GetStoreByUserId(ObjectId.Parse(userId));
            if (store == null)
                throw new KeyNotFoundException("Store not found.");

            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!store.Roles.TryGetValue(userId, out var role))
                throw new AccessDeniedException("User is not owner or manager.");

            if (role != StoreRole.Owner && role != StoreRole.Manager)
                throw new AccessDeniedException("User is not owner or manager.");

            var categories = await _categoryRepository.GetCategoryPathAsync(productDto.Category);
            if (categories == null)
                throw new KeyNotFoundException("Category not found.");

            var product = _mapper.Map<Product>(productDto);

            product.Id = ObjectId.GenerateNewId();
            product.CategoryPath = [..categories];
            product.SellerId = store.Id;
            product.QuantityStatus = product.Quantity switch
            {
                0 => QuantityStatus.OutOfStock,
                <= 4 => QuantityStatus.Ending,
                _ => QuantityStatus.InStock
            };

            _logger.LogInformation("CreateAsync success");
            await _productRepository.CreateAsync(product);
        }
    }

    /// <summary>
    ///     Updates an existing product.
    /// </summary>
    /// <param name="productDto">Product data to update.</param>
    /// <param name="userId">Product updater.</param>
    /// <returns>The updated product or null if update failed.</returns>
    public async Task UpdateAsync(UpdateProductDto productDto, string userId)
    {
        using (_logger.BeginScope("UpdateAsync")){
            _logger.LogInformation("UpdateAsync called");
            var store = await _storeRepository.GetStoreById(ObjectId.Parse(productDto.SellerId));
            if (store == null)
                throw new KeyNotFoundException("Store not found.");

            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!store.Roles.TryGetValue(userId, out var role) ||
                (role != StoreRole.Owner && role != StoreRole.Manager))
                throw new AccessDeniedException("User is not owner or manager.");

            var categories = await _categoryRepository.GetCategoryPathAsync(productDto.Category);
            if (categories == null)
                throw new KeyNotFoundException("Category not found.");
            var product = _mapper.Map<Product>(productDto);
            product.CategoryPath = categories;
            product.QuantityStatus = product.Quantity switch
            {
                0 => QuantityStatus.OutOfStock,
                <= 4 => QuantityStatus.Ending,
                _ => QuantityStatus.InStock
            };
            var success = await _productRepository.UpdateAsync(product);
            if (!success)
                throw new InvalidOperationException("Product could not be updated.");
            _logger.LogInformation("UpdateAsync success");
        }
    }

    /// <summary>
    ///     Deletes a product by ID.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <param name="userId">Product deleter</param>
    /// <returns>True if deleted successfully; otherwise, false.</returns>
    public async Task DeleteAsync(string id, string userId)
    {
        using (_logger.BeginScope("DeleteAsync")) {
            _logger.LogInformation("DeleteAsync called");
            var store = await _storeRepository.GetStoreById(ObjectId.Parse(id));
            if (store == null)
                throw new KeyNotFoundException("Store not found.");

            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(id));
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!store.Roles.TryGetValue(userId, out var role) ||
                (role != StoreRole.Owner && role != StoreRole.Manager))
                throw new AccessDeniedException("User is not owner or manager.");
            if (!await _productRepository.DeleteAsync(ObjectId.Parse(id)))
                throw new InvalidOperationException("Product could not be deleted.");
            _logger.LogInformation("DeleteAsync success");
        }
    }

    /// <summary>
    ///     Searches for products by name and language code.
    /// </summary>
    /// <param name="name">Search query.</param>
    /// <param name="languageCode">Language of the product name. Default is "en".</param>
    /// <returns>List of search results or null.</returns>
    public async Task<IEnumerable<ProductSearchResultDto>?> SearchByNameAsync(string name)
    {
        using (_logger.BeginScope("SearchByNameAsync")){
            _logger.LogInformation("SearchByNameAsync called");
            var results = await _productRepository.SearchByNameAsync(name);
            _logger.LogInformation("SearchByNameAsync success");
            return _mapper.Map<IEnumerable<ProductSearchResultDto>?>(results);
        }
    }

    public async Task<IEnumerable<ProductDto>> GetRandomProductsAsync(int page, int pageSize)
    {
        using (_logger.BeginScope("GetRandomProductsAsync")){
            _logger.LogInformation("GetRandomProductsAsync called");
            var result = await _productRepository.GetRandomProductsAsync(page, pageSize);
            _logger.LogInformation("GetRandomProductsAsync success");
            return _mapper.Map<IEnumerable<ProductDto>>(result);
        }
    }
}