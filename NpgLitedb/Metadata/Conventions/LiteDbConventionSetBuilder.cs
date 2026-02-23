using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace NpgLitedb.Metadata.Conventions;

/// <summary>
/// Convention set builder for LiteDB. Adds LiteDB-specific conventions.
/// </summary>
public class LiteDbConventionSetBuilder : ProviderConventionSetBuilder
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbConventionSetBuilder(
        ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        // LiteDB doesn't need table/schema related conventions from relational,
        // but the base set already provides the right entity/property conventions.

        return conventionSet;
    }
}
