using App.Core.DTOs.Product;
using App.Core.Interfaces;
using App.Core.Models.Product;
using AutoMapper;
using DnsClient.Protocol;
using MongoDB.Bson;
using MyApp.Core.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services;

/// <summary>
/// Service for managing products, their variations, and media.
/// </summary>
public class ProductService(IProductRepository productRepository, IMapper mapper, ICategoryRepository categoryRepository) : IProductService
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    /// <summary>
    /// Retrieves all products matching the specified filter.
    /// </summary>
    /// <param name="filter">Filtering options.</param>
    /// <returns>A list of matching products or null.</returns>
    public async Task<List<ProductDto>?> GetAllAsync(ProductFilterRequestDto filter)
    {
        var products = await _productRepository.GetAllAsync(_mapper.Map<ProductFilterRequest>(filter));
        return _mapper.Map<List<ProductDto>?>(products);
    }

    /// <summary>
    /// Retrieves a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product.</param>
    /// <returns>The matching product or null.</returns>
    public async Task<ProductDto?> GetByIdAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return _mapper.Map<ProductDto>(product);
    }

    /// <summary>
    /// Retrieves products by name and optional filter.
    /// </summary>
    /// <param name="name">Product name (partial or full).</param>
    /// <param name="filter">Additional filtering options.</param>
    /// <returns>A list of matching products or null.</returns>
    public async Task<List<ProductDto>?> GetByNameAsync(string name, ProductFilterRequestDto filter)
    {
        var products = await _productRepository.GetByNameAsync(name, _mapper.Map<ProductFilterRequest>(filter));
        return _mapper.Map<List<ProductDto>?>(products);
    }

    /// <summary>
    /// Retrieves products by category ID.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <param name="filter">Filtering options.</param>
    /// <returns>A list of products in the category or null.</returns>
    public async Task<List<ProductDto>?> GetByCategoryAsync(string categoryId, ProductFilterRequestDto filter)
    {
        var products = await _productRepository.GetByCategoryAsync(categoryId, _mapper.Map<ProductFilterRequest>(filter));
        return _mapper.Map<List<ProductDto>?>(products);
    }

    /// <summary>
    /// Retrieves products by seller ID.
    /// </summary>
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="filter">Filtering options.</param>
    /// <returns>A list of products by the seller or null.</returns>
    public async Task<List<ProductDto>?> GetBySellerIdAsync(string sellerId, ProductFilterRequestDto filter)
    {
        var products = await _productRepository.GetBySellerIdAsync(sellerId, _mapper.Map<ProductFilterRequest>(filter));
        return _mapper.Map<List<ProductDto>?>(products);
    }

    /// <summary>
    /// Retrieves a product by its variation (model) ID.
    /// </summary>
    /// <param name="modelId">The model ID of the product variation.</param>
    /// <param name="filter">Filtering options.</param>
    /// <returns>The product containing the specified variation or null.</returns>
    public async Task<ProductDto?> GetByModelIdAsync(string modelId, ProductFilterRequestDto filter)
    {
        var product = await _productRepository.GetByModelIdAsync(modelId, _mapper.Map<ProductFilterRequest>(filter));
        return _mapper.Map<ProductDto?>(product);
    }

    /// <summary>
    /// Retrieves multiple products by their variation (model) IDs.
    /// </summary>
    /// <param name="modelId">List of model IDs.</param>
    /// <param name="filter">Filtering options.</param>
    /// <returns>A list of products or null.</returns>
    public async Task<List<ProductDto>?> GetByModelIdsAsync(List<string> modelId, ProductFilterRequestDto filter)
    {
        var products = await _productRepository.GetByModelIdsAsync(modelId, _mapper.Map<ProductFilterRequest>(filter));
        return _mapper.Map<List<ProductDto>?>(products);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="productDto">Data to create the product.</param>
    /// <returns>The created product DTO.</returns>
    public async Task<ProductDto> CreateAsync(ProductCreateDto productDto)
    {
        var categories = await _categoryRepository.GetCategoryPathAsync(productDto.Category);
        var product = _mapper.Map<Product>(productDto);
        product.Id = ObjectId.GenerateNewId();
        product.CategoryPath = [..categories];
        await _productRepository.CreateAsync(product);
        return _mapper.Map<ProductDto>(product);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="productDto">Product data to update.</param>
    /// <returns>The updated product or null if update failed.</returns>
    public async Task<ProductDto?> UpdateAsync(ProductDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);
        var success = await _productRepository.UpdateAsync(product);
        if (!success) return null;
        var updatedProduct = await _productRepository.GetByIdAsync(productDto.Id);
        return _mapper.Map<ProductDto>(updatedProduct);
    }

    /// <summary>
    /// Deletes a product by ID.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <returns>True if deleted successfully; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(string id)
    {
        return await _productRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Searches for products by name and language code.
    /// </summary>
    /// <param name="name">Search query.</param>
    /// <param name="languageCode">Language of the product name. Default is "en".</param>
    /// <returns>List of search results or null.</returns>
    public async Task<List<ProductSearchResult>?> SearchByNameAsync(string name, string languageCode = "en")
    {
        return await _productRepository.SearchByNameAsync(name, languageCode);
    }

    /// <summary>
    /// Adds a variation to a product.
    /// </summary>
    /// <param name="productId">The ID of the product to add the variation to.</param>
    /// <param name="variationDto">The variation data.</param>
    /// <returns>The added variation or null if the product was not found or update failed.</returns>
    public async Task<ProductVariationDto?> AddVariationAsync(string productId, ProductVariationDto variationDto)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return null;

        var variation = _mapper.Map<ProductVariation>(variationDto);
        variation.ModelId = NanoIdGenerator.Generate(10);

        product.Variations.Add(variation);

        var success = await _productRepository.UpdateAsync(product);
        if (!success) return null;

        return _mapper.Map<ProductVariationDto>(variation);
    }

    /// <summary>
    /// Updates an existing variation of a product.
    /// </summary>
    /// <param name="productId">The ID of the product that contains the variation.</param>
    /// <param name="variationDto">The updated variation data.</param>
    /// <returns>The updated variation or null if not found or update failed.</returns>
    public async Task<ProductVariationDto?> UpdateVariationAsync(string productId, ProductVariationDto variationDto)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return null;

        var index = product.Variations.FindIndex(v => v.ModelId == variationDto.ModelId);
        if (index == -1) return null;

        var updatedVariation = _mapper.Map<ProductVariation>(variationDto);

        updatedVariation.ModelId = variationDto.ModelId;

        product.Variations[index] = updatedVariation;

        var success = await _productRepository.UpdateAsync(product);
        if (!success) return null;

        return _mapper.Map<ProductVariationDto>(updatedVariation);
    }

    /// <summary>
    /// Removes a variation from a product.
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <param name="modelId">The model ID of the variation to remove.</param>
    /// <returns>True if the variation was removed successfully; otherwise, false.</returns>
    public async Task<bool> RemoveVariationAsync(string productId, string modelId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return false;

        var removed = product.Variations.RemoveAll(v => v.ModelId == modelId) > 0;
        if (!removed) return false;

        return await _productRepository.UpdateAsync(product);
    }

} 