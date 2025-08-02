using App.Core.DTOs.AvailableFilters;
using App.Core.Interfaces;
using App.Core.Models.AvailableFilters;
using AutoMapper;

namespace App.Services;

public class AvailableFiltersService(IAvailableFiltersRepository repository, IMapper mapper) : IAvailableFiltersService
{
    private readonly IAvailableFiltersRepository _repository = repository;
    private readonly IMapper _mapper = mapper;
    
    public async Task CreateFilterCollectionAsync(AvailableFiltersCreateDto filters)
    {
        await _repository.CreateFilterCollectionAsync(_mapper.Map<AvailableFilters>(filters));
    }

    public async Task<List<AvailableFiltersDto>> GetAllFiltersAsync()
    {
        return _mapper.Map<List<AvailableFiltersDto>>(await _repository.GetAllFiltersAsync());
    }

    public async Task<List<AvailableFiltersDto>> GetAllFiltersAsync(string categoryId)
    {
        return _mapper.Map<List<AvailableFiltersDto>>(await _repository.GetAllFiltersAsync(categoryId));
    }

    public async Task<bool> RemoveCollectionByCategoryIdAsync(string categoryId)
    {
        return await _repository.RemoveCollectionByCategoryIdAsync(categoryId);
    }

    public async Task<bool> RemoveCollectionByIdAsync(string id)
    {
        return await _repository.RemoveCollectionByIdAsync(id);
    }

    public async Task AddFilterToCollectionAsync(string categoryId, List<AvailableFiltersDto> filtersDto)
    {
        await _repository.AddFilterToCollectionAsync(categoryId, _mapper.Map<List<AvailableFiltersItem>>(filtersDto));
    }

    public async Task<bool> RemoveFilterFromCollectionAsync(string categoryId, List<string> values)
    {
        return await _repository.RemoveFilterFromCollectionAsync(categoryId, values);
    }

    public async Task<bool> UpdateFilterCollectionAsync(string id, List<AvailableFiltersItemDto> filters)
    {
        return await _repository.UpdateFilterCollectionAsync(id, _mapper.Map<List<AvailableFiltersItem>>(filters));
    }

    public async Task<bool> UpdateFilterCollectionAsync(AvailableFiltersDto updatedFilters)
    {
        return await _repository.UpdateFilterCollectionAsync(_mapper.Map<AvailableFilters>(updatedFilters));
    }
}