using App.Core.Constants;
using App.Core.Interfaces;
using App.Core.Models.FileStorage;
using App.Core.Models.User;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace App.Data;

public class MongoDbSeeder(MongoDbContext context, ILogger<MongoDbSeeder> logger, IFileService fileService)
{
    private readonly MongoDbContext _dbContext = context;
    private readonly IFileService _fileService = fileService;
    private readonly ILogger<MongoDbSeeder> _logger = logger;

    public async Task SeedUserAsync()
    {
        var usersCollection = _dbContext.Users;

        var filter = Builders<User>.Filter.Eq(u => u.Username, "admin");
        var adminUserExists = await usersCollection.Find(filter).AnyAsync();
        if (adminUserExists)
        {
            _logger.LogInformation("Admin already exists. Skipping seeding.");
            return;
        }

        BaseFile file = new();
        (file.SourceUrl, file.CompressedUrl, file.SourceFileName, file.CompressedFileName) =
            await _fileService.SaveImageAsync(
                await GetImageStreamAsync(
                    "https://www.cariblist.com/admin/assets/img/UserLogos/1473851754-avatar-generic.jpg"),
                "admin-avatar",
                "user-avatars");
        var defaultUser = new User("admin", "password", "admin@sellpoint.pp.ua", file,
            [RoleNames.Admin, RoleNames.User]);

        await usersCollection.InsertOneAsync(defaultUser);
        _logger.LogInformation("Default admin user has been created.");
    }

    private async Task<Stream> GetImageStreamAsync(string imageUrl)
    {
        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }
}