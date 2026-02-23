using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace NpgLitedb.Query.Internal;

/// <summary>
/// Specifies which element operation to apply to the query result.
/// </summary>
public enum ElementOperator
{
    None,
    First,
    FirstOrDefault,
    Last,
    LastOrDefault,
    Single,
    SingleOrDefault
}

/// <summary>
/// Represents a query expression for LiteDB that holds the entity type
/// and projection information needed to execute a query against a LiteDB collection.
/// </summary>
public class LiteDbQueryExpression : Expression
{
    private readonly IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();
    private readonly List<LambdaExpression> _predicates = [];
    private readonly List<(LambdaExpression KeySelector, bool Ascending)> _orderings = [];
    private LambdaExpression? _selector;
    private bool _isCountQuery;
    private bool _isLongCountQuery;
    private ElementOperator _elementOperator;
    private LambdaExpression? _groupByKeySelector;
    private LambdaExpression? _groupByElementSelector;

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

    /// <summary>
    /// Gets the filter predicates to apply after materialization.
    /// </summary>
    public IReadOnlyList<LambdaExpression> Predicates => _predicates;

    /// <summary>
    /// Adds a filter predicate to this query.
    /// </summary>
    public void AddPredicate(LambdaExpression predicate)
        => _predicates.Add(predicate);

    /// <summary>
    /// Gets the optional projection selector to apply after materialization.
    /// </summary>
    public LambdaExpression? Selector => _selector;

    /// <summary>
    /// Sets the projection selector for this query.
    /// </summary>
    public void SetSelector(LambdaExpression selector)
        => _selector = selector;

    /// <summary>
    /// Gets the ordering expressions to apply after materialization.
    /// </summary>
    public IReadOnlyList<(LambdaExpression KeySelector, bool Ascending)> Orderings => _orderings;

    /// <summary>
    /// Adds an ordering to this query. If <paramref name="clearExisting"/> is true, removes all previous orderings first.
    /// </summary>
    public void AddOrdering(LambdaExpression keySelector, bool ascending, bool clearExisting = false)
    {
        if (clearExisting)
            _orderings.Clear();

        _orderings.Add((keySelector, ascending));
    }

    /// <summary>
    /// Gets whether this query is a Count aggregation.
    /// </summary>
    public bool IsCountQuery => _isCountQuery;

    /// <summary>
    /// Gets whether this query is a LongCount aggregation.
    /// </summary>
    public bool IsLongCountQuery => _isLongCountQuery;

    /// <summary>
    /// Marks this query as a Count aggregation.
    /// </summary>
    public void SetCount() => _isCountQuery = true;

    /// <summary>
    /// Marks this query as a LongCount aggregation.
    /// </summary>
    public void SetLongCount() => _isLongCountQuery = true;

    /// <summary>
    /// Gets the element operator to apply (First, Last, Single, etc.).
    /// </summary>
    public ElementOperator ElementOperator => _elementOperator;

    /// <summary>
    /// Sets the element operator for this query.
    /// </summary>
    public void SetElementOperator(ElementOperator op) => _elementOperator = op;

    /// <summary>
    /// Gets the GroupBy key selector.
    /// </summary>
    public LambdaExpression? GroupByKeySelector => _groupByKeySelector;

    /// <summary>
    /// Gets the GroupBy element selector.
    /// </summary>
    public LambdaExpression? GroupByElementSelector => _groupByElementSelector;

    /// <summary>
    /// Sets the GroupBy key selector (and optional element selector).
    /// </summary>
    public void SetGroupBy(LambdaExpression keySelector, LambdaExpression? elementSelector = null)
    {
        _groupByKeySelector = keySelector;
        _groupByElementSelector = elementSelector;
    }

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
