using Microsoft.EntityFrameworkCore.Storage;

namespace NpgLitedb.Storage.Internal;

/// <summary>
/// Database creator implementation for LiteDB.
/// LiteDB creates the database file automatically when first accessed.
/// </summary>
public class LiteDbDatabaseCreator : IDatabaseCreator
{
    private readonly ILiteDbConnection _connection;

    /// <summary>
    /// Creates a new instance of <see cref="LiteDbDatabaseCreator"/>.
    /// </summary>
    public LiteDbDatabaseCreator(ILiteDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public bool EnsureCreated()
    {
        // LiteDB creates the database file automatically.
        // Access the database to ensure it's created.
        _ = _connection.Database;
        return true;
    }

    /// <inheritdoc />
    public Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(EnsureCreated());
    }

    /// <inheritdoc />
    public bool EnsureDeleted()
    {
        try
        {
            var db = _connection.Database;
            // Drop all collections
            foreach (var name in db.GetCollectionNames().ToList())
            {
                db.DropCollection(name);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(EnsureDeleted());
    }

    /// <inheritdoc />
    public bool CanConnect()
    {
        try
        {
            _ = _connection.Database;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CanConnect());
    }
}
