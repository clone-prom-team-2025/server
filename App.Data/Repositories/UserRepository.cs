using App.Core.Interfaces;
using App.Core.Models.User;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

/// <summary>
///     Repository class for performing CRUD operations and queries on the Users collection in MongoDB.
/// </summary>
public class UserRepository(MongoDbContext mongoDbContext) : IUserRepository
{
    private readonly IMongoCollection<User> _users = mongoDbContext.Users;

    /// <summary>
    ///     Retrieves all users from the database.
    /// </summary>
    /// <returns>A list of all users or null if none are found.</returns>
    public async Task<List<User>?> GetAllUsersAsync()
    {
        return await _users.Find(FilterDefinition<User>.Empty).ToListAsync();
    }

    /// <summary>
    ///     Retrieves users with pagination support.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (1-based).</param>
    /// <param name="pageSize">The number of users per page.</param>
    /// <returns>A list of users in the specified page or null if none found.</returns>
    public async Task<List<User>?> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        return await _users
            .Find(FilterDefinition<User>.Empty)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    /// <summary>
    ///     Retrieves a user by their unique MongoDB ObjectId.
    /// </summary>
    /// <param name="userId">The user's ObjectId as string.</param>
    /// <returns>The user matching the ID or null if not found.</returns>
    public async Task<User?> GetUserByIdAsync(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, ObjectId.Parse(userId));

        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    /// <summary>
    ///     Retrieves a user by their username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The matching user or null.</returns>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Username, username);

        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    /// <summary>
    ///     Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The matching user or null.</returns>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Ne(u => u.Email, null),
            Builders<User>.Filter.Eq(u => u.Email, email)
        );
        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    /// <summary>
    ///     Retrieves a user by their phone number stored in additional info.
    /// </summary>
    /// <param name="phoneNumber">The phone number to search for.</param>
    /// <returns>The matching user or null.</returns>
    public async Task<User?> GetUserByPhoneNumberAsync(string phoneNumber)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Ne(u => u.AdditionalInfo, null),
            Builders<User>.Filter.Ne(u => u.AdditionalInfo!.PhoneNumber, null),
            Builders<User>.Filter.Eq(u => u.AdditionalInfo!.PhoneNumber, phoneNumber)
        );

        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    /// <summary>
    ///     Retrieves all users with the specified role.
    /// </summary>
    /// <param name="role">The role name (e.g., "Admin", "User").</param>
    /// <returns>A list of users with that role or null if the role is invalid.</returns>
    public async Task<List<User>?> GetUsersByRoleAsync(string role)
    {
        var filter = Builders<User>.Filter.AnyEq(u => u.Roles, role);

        return await _users.Find(filter).ToListAsync();
    }

    /// <summary>
    ///     Retrieves users by role with pagination support.
    /// </summary>
    /// <param name="role">The role name.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Page size.</param>
    /// <returns>A list of users matching the role and page parameters, or null if role is invalid.</returns>
    public async Task<List<User>?> GetUsersByRoleAsync(string role, int pageNumber, int pageSize)
    {
        var filter = Builders<User>.Filter.AnyEq(u => u.Roles, role);

        return await _users
            .Find(filter)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    /// <summary>
    ///     Inserts a new user into the database.
    /// </summary>
    /// <param name="user">The user object to create.</param>
    public async Task CreateUserAsync(User user)
    {
        await _users.InsertOneAsync(user);
    }

    /// <summary>
    ///     Replaces an existing user by ID.
    /// </summary>
    /// <param name="user">The updated user object.</param>
    /// <returns>True if update was acknowledged, otherwise false.</returns>
    public async Task<bool> UpdateUserAsync(User user)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
        var result = await _users.ReplaceOneAsync(filter, user);
        return result.IsAcknowledged;
    }

    /// <summary>
    ///     Deletes a user by their ID.
    /// </summary>
    /// <param name="userId">The user's ObjectId as string.</param>
    /// <returns>True if deletion was acknowledged, otherwise false.</returns>
    public async Task<bool> DeleteUserAsync(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, ObjectId.Parse(userId));
        var result = await _users.DeleteOneAsync(filter);

        return result.IsAcknowledged;
    }

    /// <summary>
    ///     Retrieves the list of roles assigned to a user.
    /// </summary>
    /// <param name="userId">The user's ObjectId as string.</param>
    /// <returns>List of user roles or empty list if not found.</returns>
    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, ObjectId.Parse(userId));
        var projection = Builders<User>.Projection.Expression(u => u.Roles);

        return await _users
            .Find(filter)
            .Project(projection)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    ///     Updates the additional info object of a user by their ID.
    /// </summary>
    /// <param name="userId">The user's ObjectId as string.</param>
    /// <param name="userAdditionalInfo">The new additional info to assign.</param>
    /// <returns>True if update was acknowledged, otherwise false.</returns>
    public async Task<bool> UpdateUserAdditionalInfoByUserIdAsync(string userId, UserAdditionalInfo userAdditionalInfo)
    {
        if (userAdditionalInfo == null) return false;

        var filter = Builders<User>.Filter.Eq(u => u.Id, ObjectId.Parse(userId));
        var update = Builders<User>.Update.Set(u => u.AdditionalInfo, userAdditionalInfo);

        var result = await _users.UpdateOneAsync(filter, update);

        return result.IsAcknowledged;
    }

    /// <summary>
    ///     Deletes the user completely by ID.
    /// </summary>
    /// <param name="userId">The user's ObjectId as string.</param>
    /// <returns>True if deleted successfully, otherwise false.</returns>
    public async Task<bool> DeleteUserAdditionalInfoByUserIdAsync(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, ObjectId.Parse(userId));
        var result = await _users.DeleteOneAsync(filter);

        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    /// <summary>
    ///     Retrieves the additional info object of a user by ID.
    /// </summary>
    /// <param name="userId">The user's ObjectId as string.</param>
    /// <returns>The user's additional info or null.</returns>
    public async Task<UserAdditionalInfo?> GetUserAdditionalInfoByUserIdAsync(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, ObjectId.Parse(userId));
        var projection = Builders<User>.Projection.Expression(u => u.AdditionalInfo);

        var additionalInfo = await _users
            .Find(filter)
            .Project(projection)
            .FirstOrDefaultAsync();

        if (additionalInfo == null) return null;

        return additionalInfo;
    }
}