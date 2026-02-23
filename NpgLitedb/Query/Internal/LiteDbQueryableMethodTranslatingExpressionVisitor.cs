using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

using System.Linq.Expressions;

namespace NpgLitedb.Query.Internal;

/// <summary>
/// Translates LINQ queryable method calls into a form that can be processed for LiteDB.
/// This visitor converts LINQ operations into <see cref="ShapedQueryExpression"/> instances
/// that use <see cref="LiteDbQueryExpression"/> as the query expression.
/// </summary>
public class LiteDbQueryableMethodTranslatingExpressionVisitor
    : QueryableMethodTranslatingExpressionVisitor
{
    private readonly QueryCompilationContext _queryCompilationContext;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext, subquery: false)
    {
        _queryCompilationContext = queryCompilationContext;
    }

    /// <summary>
    /// Creates a new instance for subquery translation.
    /// </summary>
    protected LiteDbQueryableMethodTranslatingExpressionVisitor(
        LiteDbQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor.Dependencies, parentVisitor._queryCompilationContext, subquery: true)
    {
        _queryCompilationContext = parentVisitor._queryCompilationContext;
    }

    /// <inheritdoc />
    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new LiteDbQueryableMethodTranslatingExpressionVisitor(this);

    /// <inheritdoc />
    protected override ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType)
    {
        var queryExpression = new LiteDbQueryExpression(entityType);

        var entityShaperExpression = new StructuralTypeShaperExpression(
            entityType,
            new ProjectionBindingExpression(queryExpression, new ProjectionMember(), typeof(ValueBuffer)),
            false);

        return new ShapedQueryExpression(queryExpression, entityShaperExpression);
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAny(ShapedQueryExpression source, LambdaExpression? predicate)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAverage(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateCast(ShapedQueryExpression source, Type castType)
        => source; // Pass-through

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateContains(ShapedQueryExpression source, Expression item)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        if (source.QueryExpression is LiteDbQueryExpression liteDbQuery)
        {
            if (predicate is not null)
                liteDbQuery.AddPredicate(predicate);

            liteDbQuery.SetCount();
            return source.UpdateShaperExpression(Expression.Constant(0));
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression? defaultValue)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateDistinct(ShapedQueryExpression source)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateElementAtOrDefault(ShapedQueryExpression source, Expression index, bool returnDefault)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateFirstOrDefault(ShapedQueryExpression source, LambdaExpression? predicate, Type returnType, bool returnDefault)
    {
        if (source.QueryExpression is LiteDbQueryExpression liteDbQuery)
        {
            if (predicate is not null)
                liteDbQuery.AddPredicate(predicate);

            liteDbQuery.SetElementOperator(returnDefault ? ElementOperator.FirstOrDefault : ElementOperator.First);
            return source;
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateGroupBy(ShapedQueryExpression source, LambdaExpression keySelector, LambdaExpression? elementSelector, LambdaExpression? resultSelector)
    {
        if (source.QueryExpression is not LiteDbQueryExpression liteDbQuery)
            return null;

        // Store groupBy info on the query expression
        liteDbQuery.SetGroupBy(keySelector, elementSelector);

        // For GroupBy as final operator (no resultSelector), create a GroupByShaperExpression
        // so the framework knows the result type is IGrouping<TKey, TElement>
        var groupByShaperExpression = new GroupByShaperExpression(
            keySelector,
            source);

        return source.UpdateShaperExpression(groupByShaperExpression);
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateGroupJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        => throw new InvalidOperationException(
            "GroupJoin is not supported by the LiteDB provider. " +
            "LiteDB is a document database and does not support server-side cross-collection joins.");

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        => null; // Client evaluation — use AsEnumerable() for cross-collection joins

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLastOrDefault(ShapedQueryExpression source, LambdaExpression? predicate, Type returnType, bool returnDefault)
    {
        if (source.QueryExpression is LiteDbQueryExpression liteDbQuery)
        {
            if (predicate is not null)
                liteDbQuery.AddPredicate(predicate);

            liteDbQuery.SetElementOperator(returnDefault ? ElementOperator.LastOrDefault : ElementOperator.Last);
            return source;
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLeftJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateRightJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLongCount(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        if (source.QueryExpression is LiteDbQueryExpression liteDbQuery)
        {
            if (predicate is not null)
                liteDbQuery.AddPredicate(predicate);

            liteDbQuery.SetLongCount();
            return source.UpdateShaperExpression(Expression.Constant(0L));
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMax(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMin(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateOfType(ShapedQueryExpression source, Type resultType)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateOrderBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
    {
        if (source.QueryExpression is LiteDbQueryExpression liteDbQuery)
        {
            liteDbQuery.AddOrdering(keySelector, ascending, clearExisting: true);
            return source;
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateReverse(ShapedQueryExpression source)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
    {
        if (source.QueryExpression is LiteDbQueryExpression liteDbQuery)
        {
            liteDbQuery.SetSelector(selector);
            return source;
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelectMany(ShapedQueryExpression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
        => throw new InvalidOperationException(
            "SelectMany is not supported by the LiteDB provider. " +
            "LiteDB is a document database and does not support server-side cross-collection flattening. " +
            "Use '.AsEnumerable()' or '.ToList()' before calling SelectMany to perform the operation on the client side. " +
            "Example: context.Orders.AsEnumerable().SelectMany(...)");

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector)
        => throw new InvalidOperationException(
            "SelectMany is not supported by the LiteDB provider. " +
            "LiteDB is a document database and does not support server-side cross-collection flattening. " +
            "Use '.AsEnumerable()' or '.ToList()' before calling SelectMany to perform the operation on the client side. " +
            "Example: context.Orders.AsEnumerable().SelectMany(...)");

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSingleOrDefault(ShapedQueryExpression source, LambdaExpression? predicate, Type returnType, bool returnDefault)
    {
        if (source.QueryExpression is LiteDbQueryExpression liteDbQuery)
        {
            if (predicate is not null)
                liteDbQuery.AddPredicate(predicate);

            liteDbQuery.SetElementOperator(returnDefault ? ElementOperator.SingleOrDefault : ElementOperator.Single);
            return source;
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSkip(ShapedQueryExpression source, Expression count)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSum(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateTake(ShapedQueryExpression source, Expression count)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateThenBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
    {
        if (source.QueryExpression is LiteDbQueryExpression liteDbQuery)
        {
            liteDbQuery.AddOrdering(keySelector, ascending);
            return source;
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2)
        => null; // Client evaluation

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
    {
        if (source.QueryExpression is LiteDbQueryExpression liteDbQuery)
        {
            liteDbQuery.AddPredicate(predicate);
            return source;
        }

        return null;
    }
}
