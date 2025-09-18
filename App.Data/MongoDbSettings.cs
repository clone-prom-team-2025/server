namespace App.Data;

/// <summary>
///     Represents the MongoDB connection settings used to configure the database context.
///     Typically bound from application configuration (e.g., appsettings.json).
/// </summary>
public class MongoDbSettings
{
    /// <summary>
    ///     The connection string used to connect to the MongoDB server.
    ///     Example: "mongodb://localhost:27017".
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    ///     The name of the MongoDB database to use.
    /// </summary>
    public string DatabaseName { get; set; } = null!;
}