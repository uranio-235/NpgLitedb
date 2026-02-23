using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NpgLitedb.Storage.Internal;

/// <summary>
/// Maps CLR types to LiteDB (BSON) types for Entity Framework Core.
/// </summary>
public class LiteDbTypeMappingSource : TypeMappingSource
{
    /// <summary>
    /// Creates a new instance of <see cref="LiteDbTypeMappingSource"/>.
    /// </summary>
    public LiteDbTypeMappingSource(TypeMappingSourceDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override CoreTypeMapping? FindMapping(in TypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        if (clrType == null)
        {
            return null;
        }

        clrType = Nullable.GetUnderlyingType(clrType) ?? clrType;

        if (clrType == typeof(string)
            || clrType == typeof(int)
            || clrType == typeof(long)
            || clrType == typeof(double)
            || clrType == typeof(decimal)
            || clrType == typeof(bool)
            || clrType == typeof(DateTime)
            || clrType == typeof(DateTimeOffset)
            || clrType == typeof(Guid)
            || clrType == typeof(byte[])
            || clrType == typeof(byte)
            || clrType == typeof(short)
            || clrType == typeof(float)
            || clrType == typeof(char)
            || clrType == typeof(uint)
            || clrType == typeof(ulong)
            || clrType == typeof(ushort)
            || clrType == typeof(sbyte)
            || clrType == typeof(TimeSpan)
            || clrType.IsEnum)
        {
            return new LiteDbTypeMapping(mappingInfo.ClrType!);
        }

        return null;
    }
}

/// <summary>
/// A concrete CoreTypeMapping implementation for LiteDB BSON types.
/// </summary>
public class LiteDbTypeMapping : CoreTypeMapping
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbTypeMapping(Type clrType)
        : base(new CoreTypeMappingParameters(clrType))
    {
    }

    private LiteDbTypeMapping(CoreTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override CoreTypeMapping Clone(CoreTypeMappingParameters parameters)
        => new LiteDbTypeMapping(parameters);

    /// <inheritdoc />
    public override CoreTypeMapping WithComposedConverter(
        ValueConverter? converter,
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        CoreTypeMapping? elementMapping = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null)
    {
        return new LiteDbTypeMapping(Parameters with
        {
            Converter = converter,
            Comparer = comparer,
            KeyComparer = keyComparer,
            ElementTypeMapping = elementMapping,
            JsonValueReaderWriter = jsonValueReaderWriter
        });
    }
}
