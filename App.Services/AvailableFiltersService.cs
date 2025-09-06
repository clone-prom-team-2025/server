using App.Core.DTOs.AvailableFilters;
using App.Core.Interfaces;
using App.Core.Models.AvailableFilters;
using AutoMapper;

namespace App.Services;

public class AvailableFiltersService(IAvailableFiltersRepository repository, IMapper mapper) : IAvailableFiltersService
{
    private readonly IMapper _mapper = mapper;
    private readonly IAvailableFiltersRepository _repository = repository;

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

    public async Task RemoveCollectionByCategoryIdAsync(string categoryId)
    {
        if (!await _repository.RemoveCollectionByCategoryIdAsync(categoryId))
            throw new Exception("Could not remove collection");
    }

    public async Task RemoveCollectionByIdAsync(string id)
    {
        if (!await _repository.RemoveCollectionByIdAsync(id))
            throw new Exception("Could not remove collection");
    }

    public async Task AddFilterToCollectionAsync(string categoryId, List<AvailableFiltersItemDto> filtersDto)
    {
        await _repository.AddFilterToCollectionAsync(categoryId, _mapper.Map<List<AvailableFiltersItem>>(filtersDto));
    }

    public async Task RemoveFilterFromCollectionAsync(string categoryId, List<string> values)
    {
        if (!await _repository.RemoveFilterFromCollectionAsync(categoryId, values))
            throw new Exception("Could not remove filter");
    }

    public async Task UpdateFilterCollectionAsync(string id, List<AvailableFiltersItemDto> filters)
    {
        if (!await _repository.UpdateFilterCollectionAsync(id, _mapper.Map<List<AvailableFiltersItem>>(filters)))
            throw new Exception("Could not update filter");
    }

    public async Task UpdateFilterCollectionAsync(AvailableFiltersDto updatedFilters)
    {
        if (!await _repository.UpdateFilterCollectionAsync(_mapper.Map<AvailableFilters>(updatedFilters)))
            throw new Exception("Could not update filter");
    }
}