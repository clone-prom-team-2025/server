using App.Core.DTOs.Product;
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
    IStoreRepository storeRepository) : IProductService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IStoreRepository _storeRepository = storeRepository;

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
        if (!ObjectId.TryParse(id, out var objectId))
            throw new ArgumentException("Invalid product id format.", nameof(id));
        var product = await _productRepository.GetByIdAsync(objectId);
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
        if (!ObjectId.TryParse(sellerId, out var objectId))
            throw new ArgumentException("Invalid product id format.", nameof(sellerId));
        var products = await _productRepository.GetBySellerIdAsync(objectId, _mapper.Map<ProductFilterRequest>(filter));
        return _mapper.Map<ProductFilterResponseDto?>(products);
    }

    /// <summary>
    ///     Creates a new product.
    /// </summary>
    /// <param name="productDto">Data to create the product.</param>
    /// <returns>The created product DTO.</returns>
    public async Task<bool> CreateAsync(ProductCreateDto productDto)
    {
        var store = await _storeRepository.GetStoreById(ObjectId.Parse(productDto.SellerId));
        if (store == null) return false;
        var categories = await _categoryRepository.GetCategoryPathAsync(productDto.Category);
        var product = _mapper.Map<Product>(productDto);
        product.Id = ObjectId.GenerateNewId();
        product.CategoryPath = [..categories];
        await _productRepository.CreateAsync(product);
        return true;
    }

    /// <summary>
    ///     Updates an existing product.
    /// </summary>
    /// <param name="productDto">Product data to update.</param>
    /// <returns>The updated product or null if update failed.</returns>
    public async Task<bool> UpdateAsync(ProductDto productDto)
    {
        var store = await _storeRepository.GetStoreById(ObjectId.Parse(productDto.SellerId));
        if (store == null) return false;
        var product = _mapper.Map<Product>(productDto);
        var success = await _productRepository.UpdateAsync(product);
        if (!success) return false;
        if (!ObjectId.TryParse(productDto.Id, out var objectId))
            throw new ArgumentException("Invalid product id format.", nameof(productDto.Id));
        var updatedProduct = await _productRepository.GetByIdAsync(objectId);
        return true;
    }

    /// <summary>
    ///     Deletes a product by ID.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <returns>True if deleted successfully; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            throw new ArgumentException("Invalid product id format.", nameof(id));
        return await _productRepository.DeleteAsync(objectId);
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