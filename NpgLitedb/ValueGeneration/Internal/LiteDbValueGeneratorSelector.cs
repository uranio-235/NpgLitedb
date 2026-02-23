using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace NpgLitedb.ValueGeneration.Internal;

/// <summary>
/// Value generator selector for LiteDB.
/// Selects appropriate value generators for entity properties.
/// </summary>
public class LiteDbValueGeneratorSelector : ValueGeneratorSelector
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbValueGeneratorSelector(
        ValueGeneratorSelectorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override ValueGenerator? FindForType(IProperty property, ITypeBase typeBase, Type clrType)
    {
        if (clrType == typeof(int) || clrType == typeof(long))
        {
            // LiteDB auto-generates integer IDs
            return new LiteDbIntegerValueGenerator();
        }

        if (clrType == typeof(Guid))
        {
            return new GuidValueGenerator();
        }

        if (clrType == typeof(string) && property.IsPrimaryKey())
        {
            return new StringValueGenerator();
        }

        return base.FindForType(property, typeBase, clrType);
    }
}

/// <summary>
/// Value generator for integer keys. Returns temporary values that will be
/// replaced by LiteDB's auto-increment mechanism.
/// </summary>
internal class LiteDbIntegerValueGenerator : ValueGenerator<int>
{
    private int _current;

    public override bool GeneratesTemporaryValues => true;

    public override int Next(EntityEntry entry)
    {
        return Interlocked.Decrement(ref _current);
    }
}

/// <summary>
/// Value generator for GUID keys.
/// </summary>
internal class GuidValueGenerator : ValueGenerator<Guid>
{
    public override bool GeneratesTemporaryValues => false;

    public override Guid Next(EntityEntry entry)
    {
        return Guid.NewGuid();
    }
}

/// <summary>
/// Value generator for string keys.
/// </summary>
internal class StringValueGenerator : ValueGenerator<string>
{
    public override bool GeneratesTemporaryValues => false;

    public override string Next(EntityEntry entry)
    {
        return Guid.NewGuid().ToString("N");
    }
}
