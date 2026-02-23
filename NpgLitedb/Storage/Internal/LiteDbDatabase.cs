using System.Linq.Expressions;
using LiteDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace NpgLitedb.Storage.Internal;

/// <summary>
/// The main database implementation for LiteDB that handles SaveChanges operations.
/// Translates EF Core change tracking entries into LiteDB CRUD operations.
/// </summary>
public class LiteDbDatabase : IDatabase
{
    private readonly ILiteDbConnection _connection;
    private readonly IQueryCompilationContextFactory _queryCompilationContextFactory;
    private readonly ICurrentDbContext _currentContext;

    /// <summary>
    /// Creates a new instance of <see cref="LiteDbDatabase"/>.
    /// </summary>
    public LiteDbDatabase(
        ILiteDbConnection connection,
        IQueryCompilationContextFactory queryCompilationContextFactory,
        ICurrentDbContext currentContext)
    {
        _connection = connection;
        _queryCompilationContextFactory = queryCompilationContextFactory;
        _currentContext = currentContext;
    }

    /// <inheritdoc />
    public Func<QueryContext, TResult> CompileQuery<TResult>(Expression query, bool async)
    {
        var compilationContext = _queryCompilationContextFactory.Create(async);
        var compiled = compilationContext.CreateQueryExecutor<TResult>(query);
        return compiled;
    }

    /// <inheritdoc />
    public Expression<Func<QueryContext, TResult>> CompileQueryExpression<TResult>(Expression query, bool async)
    {
        var compilationContext = _queryCompilationContextFactory.Create(async);
        var compiled = compilationContext.CreateQueryExecutor<TResult>(query);
        // Wrap the delegate into an expression
        return (QueryContext qc) => compiled(qc);
    }

    /// <inheritdoc />
    public int SaveChanges(IList<IUpdateEntry> entries)
    {
        var rowsAffected = 0;

        foreach (var entry in entries)
        {
            var entityType = entry.EntityType;
            var collectionName = GetCollectionName(entityType);
            var collection = _connection.GetCollection(collectionName);

            switch (entry.EntityState)
            {
                case EntityState.Added:
                    var insertDoc = ConvertToBsonDocument(entry);
                    collection.Insert(insertDoc);
                    PropagateGeneratedValues(entry, insertDoc);
                    rowsAffected++;
                    break;

                case EntityState.Modified:
                    var updateDoc = ConvertToBsonDocument(entry);
                    var updateId = GetBsonId(entry);
                    updateDoc["_id"] = updateId;
                    if (collection.Update(updateDoc))
                    {
                        rowsAffected++;
                    }
                    break;

                case EntityState.Deleted:
                    var deleteId = GetBsonId(entry);
                    if (collection.Delete(deleteId))
                    {
                        rowsAffected++;
                    }
                    break;
            }
        }

        return rowsAffected;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(
        IList<IUpdateEntry> entries,
        CancellationToken cancellationToken = default)
    {
        // LiteDB is synchronous, so we wrap in Task.Run
        return await Task.Run(() => SaveChanges(entries), cancellationToken);
    }

    internal static string GetCollectionName(IEntityType entityType)
    {
        return entityType.ShortName();
    }

    private static BsonDocument ConvertToBsonDocument(IUpdateEntry entry)
    {
        var doc = new BsonDocument();
        var entityType = entry.EntityType;

        foreach (var property in entityType.GetProperties())
        {
            var value = entry.GetCurrentValue(property);
            var propertyName = property.Name;

            // Map the primary key to _id for LiteDB
            if (property.IsPrimaryKey())
            {
                propertyName = "_id";
            }

            doc[propertyName] = ConvertToBsonValue(value, property.ClrType);
        }

        return doc;
    }

    private static BsonValue ConvertToBsonValue(object? value, Type clrType)
    {
        if (value is null)
        {
            return BsonValue.Null;
        }

        return value switch
        {
            int i => new BsonValue(i),
            long l => new BsonValue(l),
            double d => new BsonValue(d),
            decimal m => new BsonValue(m),
            string s => new BsonValue(s),
            bool b => new BsonValue(b),
            DateTime dt => new BsonValue(dt),
            Guid g => new BsonValue(g),
            byte[] bytes => new BsonValue(bytes),
            float f => new BsonValue((double)f),
            short sh => new BsonValue((int)sh),
            byte by => new BsonValue((int)by),
            char c => new BsonValue(c.ToString()),
            uint ui => new BsonValue((long)ui),
            ulong ul => new BsonValue((decimal)ul),
            ushort us => new BsonValue((int)us),
            sbyte sb => new BsonValue((int)sb),
            DateTimeOffset dto => new BsonValue(dto.UtcDateTime),
            TimeSpan ts => new BsonValue((long)ts.Ticks),
            Enum e => new BsonValue(Convert.ToInt32(e)),
            _ => new BsonValue(value.ToString())
        };
    }

    internal static object? ConvertFromBsonValue(BsonValue bsonValue, Type targetType)
    {
        if (bsonValue.IsNull)
        {
            return null;
        }

        targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (targetType == typeof(int)) return bsonValue.AsInt32;
        if (targetType == typeof(long)) return bsonValue.AsInt64;
        if (targetType == typeof(double)) return bsonValue.AsDouble;
        if (targetType == typeof(decimal)) return bsonValue.AsDecimal;
        if (targetType == typeof(string)) return bsonValue.AsString;
        if (targetType == typeof(bool)) return bsonValue.AsBoolean;
        if (targetType == typeof(DateTime)) return bsonValue.AsDateTime;
        if (targetType == typeof(Guid)) return bsonValue.AsGuid;
        if (targetType == typeof(byte[])) return bsonValue.AsBinary;
        if (targetType == typeof(float)) return (float)bsonValue.AsDouble;
        if (targetType == typeof(short)) return (short)bsonValue.AsInt32;
        if (targetType == typeof(byte)) return (byte)bsonValue.AsInt32;
        if (targetType == typeof(char)) return bsonValue.AsString?.FirstOrDefault() ?? '\0';
        if (targetType == typeof(uint)) return (uint)bsonValue.AsInt64;
        if (targetType == typeof(ulong)) return (ulong)bsonValue.AsDecimal;
        if (targetType == typeof(ushort)) return (ushort)bsonValue.AsInt32;
        if (targetType == typeof(sbyte)) return (sbyte)bsonValue.AsInt32;
        if (targetType == typeof(DateTimeOffset)) return new DateTimeOffset(bsonValue.AsDateTime, TimeSpan.Zero);
        if (targetType == typeof(TimeSpan)) return new TimeSpan(bsonValue.AsInt64);
        if (targetType.IsEnum) return Enum.ToObject(targetType, bsonValue.AsInt32);

        return bsonValue.RawValue;
    }

    private static BsonValue GetBsonId(IUpdateEntry entry)
    {
        var keyProperty = entry.EntityType.FindPrimaryKey()?.Properties.FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"Entity type '{entry.EntityType.DisplayName()}' does not have a primary key defined.");

        var keyValue = entry.GetCurrentValue(keyProperty);
        return ConvertToBsonValue(keyValue, keyProperty.ClrType);
    }

    private static void PropagateGeneratedValues(IUpdateEntry entry, BsonDocument doc)
    {
        var keyProperty = entry.EntityType.FindPrimaryKey()?.Properties.FirstOrDefault();
        if (keyProperty == null)
        {
            return;
        }

        // If the key was auto-generated by LiteDB (ObjectId or auto-increment),
        // propagate it back to the entity
        if (doc.TryGetValue("_id", out var idValue))
        {
            if (entry.HasTemporaryValue(keyProperty))
            {
                var convertedValue = ConvertFromBsonValue(idValue, keyProperty.ClrType);
                if (convertedValue != null)
                {
                    entry.SetStoreGeneratedValue(keyProperty, convertedValue);
                }
            }
        }
    }

    }
