using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

using NpgLitedb.Diagnostics.Internal;
using NpgLitedb.Infrastructure.Internal;
using NpgLitedb.Metadata.Conventions;
using NpgLitedb.Query.Internal;
using NpgLitedb.Storage.Internal;
using NpgLitedb.ValueGeneration.Internal;

namespace NpgLitedb.Extensions;

/// <summary>
/// LiteDB-specific extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class LiteDbServiceCollectionExtensions
{
    /// <summary>
    /// Registers the given Entity Framework <see cref="DbContext"/> as a service in the <see cref="IServiceCollection"/>
    /// and configures it to use LiteDB database provider.
    /// </summary>
    public static IServiceCollection AddEntityFrameworkLiteDb(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        new EntityFrameworkServicesBuilder(services)
            .TryAdd<LoggingDefinitions, LiteDbLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<LiteDbOptionsExtension>>()
            .TryAdd<IDatabase, LiteDbDatabase>()
            .TryAdd<IDatabaseCreator, LiteDbDatabaseCreator>()
            .TryAdd<IDbContextTransactionManager, LiteDbTransactionManager>()
            .TryAdd<ITypeMappingSource, LiteDbTypeMappingSource>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, LiteDbQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, LiteDbShapedQueryCompilingExpressionVisitorFactory>()
            .TryAdd<IQueryTranslationPreprocessorFactory, LiteDbQueryTranslationPreprocessorFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, LiteDbQueryTranslationPostprocessorFactory>()
            .TryAdd<IQueryContextFactory, LiteDbQueryContextFactory>()
            .TryAdd<IValueGeneratorSelector, LiteDbValueGeneratorSelector>()
            .TryAdd<IProviderConventionSetBuilder, LiteDbConventionSetBuilder>()
            .TryAdd<IModelValidator, LiteDbModelValidator>()
            .TryAddProviderSpecificServices(
                b => b.TryAddScoped<ILiteDbConnection, LiteDbConnection>())
            .TryAddCoreServices();

        return services;
    }
}
