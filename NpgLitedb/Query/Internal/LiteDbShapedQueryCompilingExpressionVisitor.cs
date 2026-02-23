using LiteDB;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

using NpgLitedb.Storage.Internal;

using System.Linq.Expressions;
using System.Reflection;

namespace NpgLitedb.Query.Internal;

/// <summary>
/// Compiles shaped query expressions into executable delegates that query LiteDB.
/// </summary>
public class LiteDbShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
{
    private readonly bool _isTracking;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbShapedQueryCompilingExpressionVisitor(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
        _isTracking = queryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;
    }

    /// <inheritdoc />
    protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        if (shapedQueryExpression.QueryExpression is LiteDbQueryExpression liteDbQuery)
        {
            return CompileEntityQuery(liteDbQuery);
        }

        throw new InvalidOperationException(
            $"Unknown query expression type: {shapedQueryExpression.QueryExpression.GetType().Name}");
    }

    private Expression CompileEntityQuery(LiteDbQueryExpression liteDbQuery)
    {
        var entityType = liteDbQuery.EntityType;
        var clrType = entityType.ClrType;

        var executeMethod = typeof(LiteDbQueryHelper)
            .GetMethod(nameof(LiteDbQueryHelper.ExecuteQuery), BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(clrType);

        Expression resultExpression = Expression.Call(
            executeMethod,
            QueryCompilationContext.QueryContextParameter,
            Expression.Constant(liteDbQuery.CollectionName),
            Expression.Constant(entityType.GetProperties().ToArray()),
            Expression.Constant(entityType),
            Expression.Constant(_isTracking && !liteDbQuery.IsCountQuery && !liteDbQuery.IsLongCountQuery),
            Expression.Constant(liteDbQuery.Predicates.ToArray()));

        // Apply orderings: OrderBy/ThenBy chain
        resultExpression = ApplyOrderings(resultExpression, liteDbQuery.Orderings, clrType);

        // Apply element operator (First→Take(1), Last→TakeLast(1), Single→no-op)
        resultExpression = ApplyElementOperator(resultExpression, liteDbQuery.ElementOperator, clrType);

        // Apply Count/LongCount aggregation — wrap in single-element array
        // so the framework can call GetSequenceType() on the result
        if (liteDbQuery.IsCountQuery)
        {
            var countMethod = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Enumerable.Count) && m.GetParameters().Length == 1)
                .MakeGenericMethod(clrType);

            var countCall = Expression.Call(null, countMethod, resultExpression);
            return Expression.NewArrayInit(typeof(int), countCall);
        }

        if (liteDbQuery.IsLongCountQuery)
        {
            var longCountMethod = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Enumerable.LongCount) && m.GetParameters().Length == 1)
                .MakeGenericMethod(clrType);

            var longCountCall = Expression.Call(null, longCountMethod, resultExpression);
            return Expression.NewArrayInit(typeof(long), longCountCall);
        }

        // Apply Select projection
        if (liteDbQuery.Selector is not null)
        {
            var selector = liteDbQuery.Selector;
            var selectMethod = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Enumerable.Select)
                            && m.GetParameters().Length == 2
                            && m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(clrType, selector.ReturnType);

            resultExpression = Expression.Call(null, selectMethod, resultExpression, selector);
        }

        return resultExpression;
    }

    private static Expression ApplyOrderings(
        Expression source,
        IReadOnlyList<(LambdaExpression KeySelector, bool Ascending)> orderings,
        Type elementType)
    {
        if (orderings.Count == 0)
            return source;

        var result = source;

        for (var i = 0; i < orderings.Count; i++)
        {
            var (keySelector, ascending) = orderings[i];
            var keyType = keySelector.ReturnType;

            string methodName;
            if (i == 0)
                methodName = ascending ? nameof(Enumerable.OrderBy) : nameof(Enumerable.OrderByDescending);
            else
                methodName = ascending ? nameof(Enumerable.ThenBy) : nameof(Enumerable.ThenByDescending);

            var method = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == methodName
                            && m.GetParameters().Length == 2)
                .MakeGenericMethod(elementType, keyType);

            result = Expression.Call(null, method, result, keySelector);
        }

        return result;
    }

    private static Expression ApplyElementOperator(
        Expression source,
        ElementOperator elementOperator,
        Type elementType)
    {
        if (elementOperator is ElementOperator.None
            or ElementOperator.Single
            or ElementOperator.SingleOrDefault)
        {
            return source;
        }

        // First/FirstOrDefault → Take(1), Last/LastOrDefault → TakeLast(1)
        var methodName = elementOperator is ElementOperator.First or ElementOperator.FirstOrDefault
            ? nameof(Enumerable.Take)
            : nameof(Enumerable.TakeLast);

        var method = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == methodName
                        && m.GetParameters().Length == 2
                        && m.GetParameters()[1].ParameterType == typeof(int))
            .MakeGenericMethod(elementType);

        return Expression.Call(null, method, source, Expression.Constant(1));
    }
}

/// <summary>
/// Helper class for executing LiteDB queries from compiled expression trees.
/// </summary>
public static class LiteDbQueryHelper
{
    /// <summary>
    /// Executes a query against a LiteDB collection and returns materialized entities.
    /// </summary>
    public static IEnumerable<T> ExecuteQuery<T>(
        QueryContext queryContext,
        string collectionName,
        IProperty[] properties,
        IEntityType entityType,
        bool isTracking,
        LambdaExpression[] predicates)
    {
        var connection = queryContext.Context.GetService<ILiteDbConnection>();
        var collection = connection.GetCollection(collectionName);
        var documents = collection.FindAll();

        var stateManager = queryContext.Context.GetService<IStateManager>();
        var keyProperties = entityType.FindPrimaryKey()?.Properties.ToArray();

        // Compile predicates into typed delegates once
        var compiledFilters = new List<Func<T, bool>>(predicates.Length);
        foreach (var predicate in predicates)
        {
            compiledFilters.Add((Func<T, bool>)predicate.Compile());
        }

        foreach (var doc in documents)
        {
            // Materialize all property values from the BsonDocument
            var propertyValues = new Dictionary<IProperty, object?>();
            foreach (var property in properties)
            {
                var propertyName = property.IsPrimaryKey()
                    ? "_id"
                    : property.Name;

                if (doc.TryGetValue(propertyName, out var bsonValue))
                {
                    propertyValues[property] = LiteDbDatabase.ConvertFromBsonValue(bsonValue, property.ClrType);
                }
                else if (property.ClrType.IsValueType)
                {
                    propertyValues[property] = Activator.CreateInstance(property.ClrType);
                }
                else
                {
                    propertyValues[property] = null;
                }
            }

            if (isTracking && keyProperties is { Length: > 0 })
            {
                // Build key values to check if already tracked
                var keyValues = new object?[keyProperties.Length];
                for (var i = 0; i < keyProperties.Length; i++)
                {
                    propertyValues.TryGetValue(keyProperties[i], out keyValues[i]);
                }

                // Check for existing tracked entity
                var key = entityType.FindPrimaryKey()!;
                var existingEntry = stateManager.TryGetEntry(key, keyValues);
                if (existingEntry != null)
                {
                    var tracked = (T)existingEntry.Entity;
                    if (PassesFilters(tracked, compiledFilters))
                    {
                        yield return tracked;
                    }

                    continue;
                }
            }

            // Create new entity instance
            var entity = (T)Activator.CreateInstance(typeof(T))!;
            foreach (var (property, value) in propertyValues)
            {
                property.PropertyInfo?.SetValue(entity, value);
            }

            // Apply filter predicates
            if (!PassesFilters(entity, compiledFilters))
            {
                continue;
            }

            if (isTracking)
            {
                var entry = stateManager.GetOrCreateEntry(entity!, entityType);
                entry.SetEntityState(EntityState.Unchanged);
            }

            yield return entity;
        }
    }

    private static bool PassesFilters<T>(T entity, List<Func<T, bool>> filters)
    {
        foreach (var filter in filters)
        {
            if (!filter(entity))
            {
                return false;
            }
        }

        return true;
    }
}
