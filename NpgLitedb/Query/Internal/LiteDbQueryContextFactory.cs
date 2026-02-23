using Microsoft.EntityFrameworkCore.Query;

namespace NpgLitedb.Query.Internal;

/// <summary>
/// A concrete <see cref="QueryContext"/> for LiteDB queries.
/// </summary>
public class LiteDbQueryContext : QueryContext
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbQueryContext(QueryContextDependencies dependencies)
        : base(dependencies)
    {
    }
}

/// <summary>
/// Factory for creating <see cref="LiteDbQueryContext"/> instances.
/// </summary>
public class LiteDbQueryContextFactory : IQueryContextFactory
{
    private readonly QueryContextDependencies _dependencies;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbQueryContextFactory(QueryContextDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public virtual QueryContext Create()
        => new LiteDbQueryContext(_dependencies);
}
