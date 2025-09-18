using App.Core.Interfaces;
using App.Core.Models.AvailableFilters;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class AvailableFiltersRepository(MongoDbContext mongoDbContext) : IAvailableFiltersRepository
{
    private readonly IMongoCollection<AvailableFilters> _availableFilters = mongoDbContext.AvailableFilters;

    public async Task CreateFilterCollectionAsync(AvailableFilters filters)
    {
        await _availableFilters.InsertOneAsync(filters);
    }

    public async Task<List<AvailableFilters>> GetAllFiltersAsync()
    {
        return await _availableFilters.Find(FilterDefinition<AvailableFilters>.Empty).ToListAsync();
    }

    public async Task<List<AvailableFilters>> GetAllFiltersAsync(string categoryId)
    {
        var filter = Builders<AvailableFilters>.Filter.Eq(af => af.CategoryId, ObjectId.Parse(categoryId));
        return await _availableFilters.Find(filter).ToListAsync();
    }

    public async Task<bool> RemoveCollectionByCategoryIdAsync(string categoryId)
    {
        var filter = Builders<AvailableFilters>.Filter.Eq(af => af.CategoryId, ObjectId.Parse(categoryId));
        var success = await _availableFilters.DeleteOneAsync(filter);
        return success.IsAcknowledged && success.DeletedCount > 0;
    }

    public async Task<bool> RemoveCollectionByIdAsync(string id)
    {
        var filter = Builders<AvailableFilters>.Filter.Eq(af => af.Id, ObjectId.Parse(id));
        var success = await _availableFilters.DeleteOneAsync(filter);
        return success.IsAcknowledged && success.DeletedCount > 0;
    }

    public async Task AddFilterToCollectionAsync(string categoryId, List<AvailableFiltersItem> newFilters)
    {
        var filter = Builders<AvailableFilters>.Filter.Eq(af => af.CategoryId, ObjectId.Parse(categoryId));
        var availableFilter = await _availableFilters.Find(filter).FirstOrDefaultAsync();

        if (availableFilter == null)
        {
            availableFilter = new AvailableFilters(ObjectId.Parse(categoryId), newFilters);
            await _availableFilters.InsertOneAsync(availableFilter);
            return;
        }

        foreach (var newFilter in newFilters)
        {
            var existing = availableFilter.Filters.FirstOrDefault(f => f.Title == newFilter.Title);
            if (existing != null)
            {
                foreach (var value in newFilter.Values)
                    if (!existing.Values.Contains(value))
                        existing.Values.Add(value);
            }
            else
            {
                availableFilter.Filters.Add(newFilter);
            }
        }

        await _availableFilters.ReplaceOneAsync(filter, availableFilter);
    }


    public async Task<bool> RemoveFilterFromCollectionAsync(string categoryId, List<string> values)
    {
        var filter = Builders<AvailableFilters>.Filter.Eq(af => af.CategoryId, ObjectId.Parse(categoryId));
        var availableFilters = await _availableFilters.Find(filter).FirstOrDefaultAsync();

        if (availableFilters == null)
            return false;

        foreach (var filterItem in availableFilters.Filters) filterItem.Values.RemoveAll(v => values.Contains(v));

        availableFilters.Filters.RemoveAll(f => f.Values == null || f.Values.Count == 0);

        var result = await _availableFilters.ReplaceOneAsync(filter, availableFilters);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateFilterCollectionAsync(string id, List<AvailableFiltersItem> filters)
    {
        var filter = Builders<AvailableFilters>.Filter.Eq(af => af.Id, ObjectId.Parse(id));
        var update = Builders<AvailableFilters>.Update.Set(af => af.Filters, filters);

        var result = await _availableFilters.UpdateOneAsync(filter, update);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateFilterCollectionAsync(AvailableFilters updatedFilters)
    {
        var filter = Builders<AvailableFilters>.Filter.Eq(af => af.Id, updatedFilters.Id);
        var result = await _availableFilters.ReplaceOneAsync(filter, updatedFilters);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
}