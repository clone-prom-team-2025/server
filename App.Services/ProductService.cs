using App.Core.DTOs.Product;
using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.Product;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services;

/// <summary>
///     Service for managing products, their variations, and media.
/// </summary>
public class ProductService(
    IProductRepository productRepository,
    IMapper mapper,
    ICategoryRepository categoryRepository,
    IStoreRepository storeRepository,
    IUserRepository userRepository) : IProductService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IStoreRepository _storeRepository = storeRepository;
    private readonly IUserRepository _userRepository = userRepository;

    /// <summary>
    ///     Retrieves all products matching the specified filter.
    /// </summary>
    /// <param name="filter">Filtering options.</param>
    /// <returns>A list of matching products or null.</returns>
    public async Task<ProductFilterResponseDto?> GetAllAsync(ProductFilterRequestDto filter)
    {
        var products = await _productRepository.GetAllAsync(_mapper.Map<ProductFilterRequest>(filter));
        return _mapper.Map<ProductFilterResponseDto?>(products);
    }

    /// <summary>
    ///     Retrieves a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product.</param>
    /// <returns>The matching product or null.</returns>
    public async Task<ProductDto?> GetByIdAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(ObjectId.Parse(id));
        return _mapper.Map<ProductDto>(product);
    }

    /// <summary>
    ///     Retrieves products by name and optional filter.
    /// </summary>
    /// <param name="name">Product name (partial or full).</param>
    /// <param name="filter">Additional filtering options.</param>
    /// <returns>A list of matching products or null.</returns>
    public async Task<ProductFilterResponseDto?> GetByNameAsync(string name, ProductFilterRequestDto filter)
    {
        var products = await _productRepository.GetByNameAsync(name, _mapper.Map<ProductFilterRequest>(filter));
        return _mapper.Map<ProductFilterResponseDto?>(products);
    }

    /// <summary>
    ///     Retrieves products by seller ID.
    /// </summary>
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="filter">Filtering options.</param>
    /// <returns>A list of products by the seller or null.</returns>
    public async Task<ProductFilterResponseDto?> GetBySellerIdAsync(string sellerId, ProductFilterRequestDto filter)
    {
        var products = await _productRepository.GetBySellerIdAsync(ObjectId.Parse(sellerId), _mapper.Map<ProductFilterRequest>(filter));
        return _mapper.Map<ProductFilterResponseDto?>(products);
    }

    /// <summary>
    ///     Creates a new product.
    /// </summary>
    /// <param name="productDto">Data to create the product.</param>
    /// <param name="userId">Product creator.</param>
    /// <returns>The created product DTO.</returns>
    public async Task CreateAsync(ProductCreateDto productDto, string userId)
    {
        var store = await _storeRepository.GetStoreById(ObjectId.Parse(productDto.SellerId));
        if (store == null)
            throw new Exception("Store not found.");
        
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null)
            throw new Exception("User not found.");
        
        if (!store.Roles.TryGetValue(userId, out var role))
            throw new Exception("User is not owner or manager.");

        if (role != StoreRole.Owner && role != StoreRole.Manager)
            throw new Exception("User is not owner or manager.");

        var categories = await _categoryRepository.GetCategoryPathAsync(productDto.Category);
        if (categories == null)
            throw new Exception("Category not found.");

        var product = _mapper.Map<Product>(productDto);

        product.Id = ObjectId.GenerateNewId();
        product.CategoryPath = [..categories];

        await _productRepository.CreateAsync(product);
    }

    /// <summary>
    ///     Updates an existing product.
    /// </summary>
    /// <param name="productDto">Product data to update.</param>
    /// <param name="userId">Product updater.</param>
    /// <returns>The updated product or null if update failed.</returns>
    public async Task UpdateAsync(UpdateProductDto productDto, string userId)
    {
        var store = await _storeRepository.GetStoreById(ObjectId.Parse(productDto.SellerId));
        if (store == null)
            throw new Exception("Store not found.");
        
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null)
            throw new Exception("User not found.");
        
        if (!store.Roles.TryGetValue(userId.ToString(), out var role) || role != StoreRole.Owner && role != StoreRole.Manager)
            throw new Exception("User is not owner or manager.");
        
        var categories = await _categoryRepository.GetCategoryPathAsync(productDto.Category);
        if (categories == null)
            throw new Exception("Category not found.");
        var product = _mapper.Map<Product>(productDto);
        product.CategoryPath = categories;
        var success = await _productRepository.UpdateAsync(product);
        if (!success) 
            throw new Exception("Product could not be updated.");
    }

    /// <summary>
    ///     Deletes a product by ID.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <param name="userId">Product deleter</param>
    /// <returns>True if deleted successfully; otherwise, false.</returns>
    public async Task DeleteAsync(string id, string userId)
    {
        var store = await _storeRepository.GetStoreById(ObjectId.Parse(id));
        if (store == null)
            throw new Exception("Store not found.");
        
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(id));
        if (user == null)
            throw new Exception("User not found.");
        
        if (!store.Roles.TryGetValue(userId.ToString(), out var role) || role != StoreRole.Owner && role != StoreRole.Manager)
            throw new Exception("User is not owner or manager.");
        if (!await _productRepository.DeleteAsync(ObjectId.Parse(id)))
        {
            throw new Exception("Product could not be deleted.");
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
        return _mapper.Map<IEnumerable<ProductSearchResultDto>?>(await _productRepository.SearchByNameAsync(name));
    }
}