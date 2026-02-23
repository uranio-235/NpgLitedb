using Microsoft.EntityFrameworkCore.Query;

namespace NpgLitedb.Query.Internal;

/// <summary>
/// Factory for creating query translation preprocessors for LiteDB.
/// </summary>
public class LiteDbQueryTranslationPreprocessorFactory : IQueryTranslationPreprocessorFactory
{
    private readonly QueryTranslationPreprocessorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbQueryTranslationPreprocessorFactory(
        QueryTranslationPreprocessorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public QueryTranslationPreprocessor Create(QueryCompilationContext queryCompilationContext)
    {
        return new LiteDbQueryTranslationPreprocessor(_dependencies, queryCompilationContext);
    }
}

/// <summary>
/// A preprocessor for LiteDB query translation.
/// </summary>
public class LiteDbQueryTranslationPreprocessor : QueryTranslationPreprocessor
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbQueryTranslationPreprocessor(
        QueryTranslationPreprocessorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
    }
}
