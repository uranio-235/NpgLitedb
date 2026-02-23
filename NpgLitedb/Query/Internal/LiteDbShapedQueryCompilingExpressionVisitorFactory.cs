using Microsoft.EntityFrameworkCore.Query;

namespace NpgLitedb.Query.Internal;

/// <summary>
/// Factory for creating <see cref="LiteDbShapedQueryCompilingExpressionVisitor"/> instances.
/// </summary>
public class LiteDbShapedQueryCompilingExpressionVisitorFactory
    : IShapedQueryCompilingExpressionVisitorFactory
{
    private readonly ShapedQueryCompilingExpressionVisitorDependencies _dependencies;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbShapedQueryCompilingExpressionVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc />
    public ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
    {
        return new LiteDbShapedQueryCompilingExpressionVisitor(
            _dependencies,
            queryCompilationContext);
    }
}
