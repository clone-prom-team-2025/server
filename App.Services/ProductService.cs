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
public class ProductService(IProductRepository productRepository, IMapper mapper) : IProductService
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<List<ProductDto>?> GetAllAsync(ProductFilterRequest filter)
    {
        var products = await _productRepository.GetAllAsync(filter);
        return _mapper.Map<List<ProductDto>?>(products);
    }

    public async Task<ProductDto?> GetByIdAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<List<ProductDto>?> GetByNameAsync(string name, ProductFilterRequest filter)
    {
        var products = await _productRepository.GetByNameAsync(name, filter);
        return _mapper.Map<List<ProductDto>?>(products);
    }

    public async Task<List<ProductDto>?> GetByCategoryAsync(string categoryId, ProductFilterRequest filter)
    {
        var products = await _productRepository.GetByCategoryAsync(categoryId, filter);
        return _mapper.Map<List<ProductDto>?>(products);
    }

    public async Task<List<ProductDto>?> GetBySellerIdAsync(string sellerId, ProductFilterRequest filter)
    {
        var products = await _productRepository.GetBySellerIdAsync(sellerId, filter);
        return _mapper.Map<List<ProductDto>?>(products);
    }

    public async Task<ProductDto?> GetByModelIdAsync(string modelId, ProductFilterRequest filter)
    {
        var product = await _productRepository.GetByModelIdAsync(modelId, filter);
        return _mapper.Map<ProductDto?>(product);
    }

    public async Task<List<ProductDto>?> GetByModelIdsAsync(List<string> modelId, ProductFilterRequest filter)
    {
        var products = await _productRepository.GetByModelIdsAsync(modelId, filter);
        return _mapper.Map<List<ProductDto>?>(products);
    }

    public async Task<ProductDto> CreateAsync(ProductCreateDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);
        product.Id = ObjectId.GenerateNewId();
        await _productRepository.CreateAsync(product);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto?> UpdateAsync(ProductDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);
        var success = await _productRepository.UpdateAsync(product);
        if (!success) return null;
        var updatedProduct = await _productRepository.GetByIdAsync(productDto.Id);
        return _mapper.Map<ProductDto>(updatedProduct);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _productRepository.DeleteAsync(id);
    }

    public async Task<List<ProductSearchResult>?> SearchByNameAsync(string name, string languageCode = "en")
    {
        return await _productRepository.SearchByNameAsync(name, languageCode);
    }

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

    public async Task<bool> RemoveVariationAsync(string productId, string modelId)
{
    var product = await _productRepository.GetByIdAsync(productId);
    if (product == null) return false;

    var removed = product.Variations.RemoveAll(v => v.ModelId == modelId) > 0;
    if (!removed) return false;

    return await _productRepository.UpdateAsync(product);
}

} 