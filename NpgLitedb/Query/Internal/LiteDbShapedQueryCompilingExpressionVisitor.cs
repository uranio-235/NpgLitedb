using System.Linq.Expressions;
using System.Reflection;
using LiteDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using NpgLitedb.Storage.Internal;

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
            var entityType = liteDbQuery.EntityType;
            var clrType = entityType.ClrType;

            var executeMethod = typeof(LiteDbQueryHelper)
                .GetMethod(nameof(LiteDbQueryHelper.ExecuteQuery), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(clrType);

            // Use the framework's QueryContext parameter - the framework wraps this in a lambda
            var callExpression = Expression.Call(
                executeMethod,
                QueryCompilationContext.QueryContextParameter,
                Expression.Constant(liteDbQuery.CollectionName),
                Expression.Constant(entityType.GetProperties().ToArray()),
                Expression.Constant(entityType),
                Expression.Constant(_isTracking));

            return callExpression;
        }

        throw new InvalidOperationException(
            $"Unknown query expression type: {shapedQueryExpression.QueryExpression.GetType().Name}");
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
        bool isTracking)
    {
        var connection = queryContext.Context.GetService<ILiteDbConnection>();
        var collection = connection.GetCollection(collectionName);
        var documents = collection.FindAll();

        var stateManager = queryContext.Context.GetService<IStateManager>();
        var keyProperties = entityType.FindPrimaryKey()?.Properties.ToArray();

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
                    yield return (T)existingEntry.Entity;
                    continue;
                }
            }

            // Create new entity instance
            var entity = (T)Activator.CreateInstance(typeof(T))!;
            foreach (var (property, value) in propertyValues)
            {
                property.PropertyInfo?.SetValue(entity, value);
            }

            if (isTracking)
            {
                var entry = stateManager.GetOrCreateEntry(entity!, entityType);
                entry.SetEntityState(EntityState.Unchanged);
            }

            yield return entity;
        }
    }
}
