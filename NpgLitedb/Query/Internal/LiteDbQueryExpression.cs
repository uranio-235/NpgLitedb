using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace NpgLitedb.Query.Internal;

/// <summary>
/// Represents a query expression for LiteDB that holds the entity type
/// and projection information needed to execute a query against a LiteDB collection.
/// </summary>
public class LiteDbQueryExpression : Expression
{
    private readonly IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();

    /// <summary>
    /// Creates a new instance of <see cref="LiteDbQueryExpression"/>.
    /// </summary>
    public LiteDbQueryExpression(IEntityType entityType)
    {
        EntityType = entityType;
        CollectionName = entityType.ShortName();

        // Set up default projection mapping - a ValueBuffer with all properties
        _projectionMapping[new ProjectionMember()] = CreateReadValueExpression(entityType);
    }

    /// <summary>
    /// Gets the entity type being queried.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    /// Gets the LiteDB collection name.
    /// </summary>
    public virtual string CollectionName { get; }

    /// <inheritdoc />
    public override Type Type => typeof(IEnumerable<ValueBuffer>);

    /// <inheritdoc />
    public override ExpressionType NodeType => ExpressionType.Extension;

    /// <summary>
    /// Gets the projection mapping.
    /// </summary>
    public virtual IDictionary<ProjectionMember, Expression> ProjectionMapping => _projectionMapping;

    private static Expression CreateReadValueExpression(IEntityType entityType)
    {
        // Return a constant expression that acts as a placeholder for the ValueBuffer
        return Constant(new ValueBuffer(), typeof(ValueBuffer));
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

    /// <summary>
    /// Gets the properties of the entity type in order.
    /// </summary>
    public IReadOnlyList<IProperty> GetProperties()
        => EntityType.GetProperties().ToList();
}
