using Microsoft.EntityFrameworkCore.Query;

namespace NpgLitedb.Query.Internal;

/// <summary>
/// Factory for creating <see cref="LiteDbQueryableMethodTranslatingExpressionVisitor"/> instances.
/// </summary>
public class LiteDbQueryableMethodTranslatingExpressionVisitorFactory
    : IQueryableMethodTranslatingExpressionVisitorFactory
{
    private readonly QueryableMethodTranslatingExpressionVisitorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance of <see cref="LiteDbQueryableMethodTranslatingExpressionVisitorFactory"/>.
    /// </summary>
    public LiteDbQueryableMethodTranslatingExpressionVisitorFactory(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public QueryableMethodTranslatingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
    {
        return new LiteDbQueryableMethodTranslatingExpressionVisitor(_dependencies, queryCompilationContext);
    }
}
