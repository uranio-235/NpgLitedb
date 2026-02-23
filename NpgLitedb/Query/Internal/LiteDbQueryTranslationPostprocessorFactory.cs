using Microsoft.EntityFrameworkCore.Query;

namespace NpgLitedb.Query.Internal;

/// <summary>
/// Factory for creating query translation postprocessors for LiteDB.
/// </summary>
public class LiteDbQueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
{
    private readonly QueryTranslationPostprocessorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbQueryTranslationPostprocessorFactory(
        QueryTranslationPostprocessorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
    {
        return new LiteDbQueryTranslationPostprocessor(_dependencies, queryCompilationContext);
    }
}

/// <summary>
/// A postprocessor for LiteDB query translation.
/// </summary>
public class LiteDbQueryTranslationPostprocessor : QueryTranslationPostprocessor
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
    }
}
