using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NpgLitedb.Extensions;

namespace NpgLitedb.Infrastructure.Internal;

/// <summary>
/// Represents options managed by the LiteDB provider.
/// This is an internal API that supports the EF Core infrastructure.
/// </summary>
public class LiteDbOptionsExtension : IDbContextOptionsExtension
{
    private string _connectionString = string.Empty;
    private DbContextOptionsExtensionInfo? _info;

    /// <summary>
    /// Creates a new instance of <see cref="LiteDbOptionsExtension"/>.
    /// </summary>
    public LiteDbOptionsExtension()
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="LiteDbOptionsExtension"/>, copying settings from an existing instance.
    /// </summary>
    protected LiteDbOptionsExtension(LiteDbOptionsExtension copyFrom)
    {
        _connectionString = copyFrom._connectionString;
    }

    /// <summary>
    /// Gets the connection string for the LiteDB database.
    /// </summary>
    public virtual string ConnectionString => _connectionString;

    /// <summary>
    /// Returns a copy of this extension with the specified connection string.
    /// </summary>
    public virtual LiteDbOptionsExtension WithConnectionString(string connectionString)
    {
        var clone = Clone();
        clone._connectionString = connectionString;
        return clone;
    }

    /// <summary>
    /// Creates a clone of this extension.
    /// </summary>
    protected virtual LiteDbOptionsExtension Clone() => new(this);

    /// <inheritdoc />
    public virtual DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    /// <inheritdoc />
    public virtual void ApplyServices(IServiceCollection services)
    {
        services.AddEntityFrameworkLiteDb();
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("A connection string must be specified for the LiteDB provider.");
        }
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private string? _logFragment;

        public ExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        private new LiteDbOptionsExtension Extension => (LiteDbOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider => true;

        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();
                    builder.Append("UseLiteDb(");

                    if (!string.IsNullOrEmpty(Extension.ConnectionString))
                    {
                        builder.Append(Extension.ConnectionString);
                    }

                    builder.Append(')');
                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }

        public override int GetServiceProviderHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Extension.ConnectionString);
            return hashCode.ToHashCode();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo
               && Extension.ConnectionString == otherInfo.Extension.ConnectionString;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["LiteDb:ConnectionString"] =
                (Extension.ConnectionString?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);
        }
    }
}
