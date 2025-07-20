using App.Core.DTOs.Product;
using App.Core.Interfaces;
using App.Core.Models.Product;
using AutoMapper;
using DnsClient.Protocol;
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
        
    }

    public async Task<ProductDto?> GetByModelIdAsync(string modelId, ProductFilterRequest filter)
    {

    }

    public async Task<List<ProductDto>?> GetByModelIdsAsync(List<string> modelId, ProductFilterRequest filter)
    {

    }

    public async Task<ProductDto> CreateAsync(ProductCreateDto product)
    {

    }

    public async Task<ProductDto> UpdateAsync(ProductDto product)
    {

    }

    public async Task<bool> DeleteAsync(string id)
    {

    }

    public async Task<List<ProductSearchResult>?> SearchByNameAsync(string name, string languageCode = "en")
    {

    }
} 