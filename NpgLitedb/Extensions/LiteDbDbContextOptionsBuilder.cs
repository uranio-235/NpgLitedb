using Microsoft.EntityFrameworkCore;

namespace NpgLitedb.Extensions;

/// <summary>
/// Allows LiteDB-specific configuration to be performed on <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public class LiteDbDbContextOptionsBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LiteDbDbContextOptionsBuilder"/> class.
    /// </summary>
    /// <param name="optionsBuilder">The core options builder.</param>
    public LiteDbDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        OptionsBuilder = optionsBuilder;
    }

    /// <summary>
    /// Gets the core options builder.
    /// </summary>
    protected virtual DbContextOptionsBuilder OptionsBuilder { get; }
}
