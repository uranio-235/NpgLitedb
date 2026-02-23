using LiteDB;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NpgLitedb.Infrastructure.Internal;

namespace NpgLitedb.Storage.Internal;

/// <summary>
/// Abstraction for the LiteDB database connection, managing the <see cref="LiteDatabase"/> instance.
/// </summary>
public interface ILiteDbConnection
{
    /// <summary>
    /// Gets the underlying <see cref="LiteDatabase"/> instance.
    /// </summary>
    LiteDatabase Database { get; }

    /// <summary>
    /// Gets a typed collection from the LiteDB database.
    /// </summary>
    ILiteCollection<BsonDocument> GetCollection(string name);
}

/// <summary>
/// Default implementation of <see cref="ILiteDbConnection"/>.
/// </summary>
public class LiteDbConnection : ILiteDbConnection, IDisposable
{
    private LiteDatabase? _database;
    private readonly string _connectionString;

    /// <summary>
    /// Creates a new instance of <see cref="LiteDbConnection"/>.
    /// </summary>
    public LiteDbConnection(IDbContextOptions options)
    {
        var extension = options.Extensions
            .OfType<LiteDbOptionsExtension>()
            .FirstOrDefault()
            ?? throw new InvalidOperationException("LiteDB options extension not found.");

        _connectionString = extension.ConnectionString;
    }

    /// <inheritdoc />
    public LiteDatabase Database
    {
        get
        {
            _database ??= new LiteDatabase(_connectionString);
            return _database;
        }
    }

    /// <inheritdoc />
    public ILiteCollection<BsonDocument> GetCollection(string name)
        => Database.GetCollection(name);

    /// <inheritdoc />
    public void Dispose()
    {
        _database?.Dispose();
        _database = null;
        GC.SuppressFinalize(this);
    }
}
