using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NpgLitedb.Storage.Internal;

/// <summary>
/// Model validator for LiteDB. Validates the model for LiteDB compatibility.
/// </summary>
public class LiteDbModelValidator : ModelValidator
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LiteDbModelValidator(
        ModelValidatorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.Validate(model, logger);

        ValidateNoSequences(model);
    }

    /// <summary>
    /// Validates that no sequences are defined (LiteDB doesn't support sequences).
    /// </summary>
    private static void ValidateNoSequences(IModel model)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.FindAnnotation("Relational:DefaultValueSql") != null)
                {
                    throw new InvalidOperationException(
                        $"LiteDB does not support SQL default values. Property: '{property.Name}' on entity '{entityType.DisplayName()}'.");
                }

                if (property.FindAnnotation("Relational:ComputedColumnSql") != null)
                {
                    throw new InvalidOperationException(
                        $"LiteDB does not support computed columns. Property: '{property.Name}' on entity '{entityType.DisplayName()}'.");
                }
            }
        }
    }
}
