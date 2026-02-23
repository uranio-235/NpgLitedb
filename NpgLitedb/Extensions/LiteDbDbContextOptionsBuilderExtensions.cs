using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NpgLitedb.Infrastructure.Internal;

namespace NpgLitedb.Extensions;

/// <summary>
/// LiteDB-specific extension methods for <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public static class LiteDbDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configures the context to connect to a LiteDB database.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connectionString">
    /// The connection string of the database to connect to. This can be a file path
    /// (e.g., "MyData.db") or a full LiteDB connection string.
    /// </param>
    /// <param name="liteDbOptionsAction">An optional action to allow additional LiteDB-specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseLiteDb(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        Action<LiteDbDbContextOptionsBuilder>? liteDbOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var extension = GetOrCreateExtension(optionsBuilder)
            .WithConnectionString(connectionString);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        liteDbOptionsAction?.Invoke(new LiteDbDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }

    /// <summary>
    /// Configures the context to connect to a LiteDB database.
    /// </summary>
    public static DbContextOptionsBuilder<TContext> UseLiteDb<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string connectionString,
        Action<LiteDbDbContextOptionsBuilder>? liteDbOptionsAction = null)
        where TContext : DbContext
    {
        return (DbContextOptionsBuilder<TContext>)UseLiteDb(
            (DbContextOptionsBuilder)optionsBuilder, connectionString, liteDbOptionsAction);
    }

    private static LiteDbOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.Options.FindExtension<LiteDbOptionsExtension>()
            ?? new LiteDbOptionsExtension();
    }
}
