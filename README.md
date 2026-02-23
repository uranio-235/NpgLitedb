# NpgLitedb — Entity Framework Core provider for LiteDB (experimental)

Lightweight EF Core provider enabling LiteDB as a backing store for Entity Framework Core. This project is experimental and intended for learning and prototyping.

## Overview

`NpgLitedb` implements a non-relational EF Core provider that allows using [LiteDB](https://www.litedb.org/) (single-file, serverless NoSQL document database) with EF Core 10. It implements provider registration, an `IDatabase` implementation, query compilation via expression trees, type mapping, value generation, and a full materialization pipeline.

Key points:
- Targets **.NET 10** and **EF Core 10**
- Uses **LiteDB 5.0.21** as the persistence engine
- Server-side translation for common LINQ operators; remaining operators fall back to client evaluation
- Design inspired by the official [EF Core Cosmos DB provider](https://github.com/dotnet/efcore)

## Features

- Configure `DbContext` with `UseLiteDb(path)`
- `EnsureCreated` / `EnsureDeleted` support
- Full CRUD via `DbSet<T>` and `SaveChanges()` / `SaveChangesAsync()`
- Auto-increment key propagation on insert
- Change-tracking integration
- Server-side LINQ translation (see table below)

## LINQ operator support

The following table describes the translation status of every LINQ operator handled by the EF Core query pipeline.

### ✅ Implemented (server-side translation)

| Operator | Notes |
|---|---|
| `Where` | Predicates stored and compiled post-materialization. Supports chaining. |
| `Select` | Projections to anonymous types, single properties, and computations. |
| `OrderBy` / `OrderByDescending` | Compiled as `Enumerable.OrderBy`/`OrderByDescending`. |
| `ThenBy` / `ThenByDescending` | Appends secondary orderings. |
| `First` / `FirstOrDefault` | With optional predicate. Limits result via `Take(1)`. |
| `Single` / `SingleOrDefault` | With optional predicate. Framework validates cardinality. |
| `Last` / `LastOrDefault` | With optional predicate. Limits result via `TakeLast(1)`. |
| `Count` | With optional predicate. Returns scalar wrapped in array for framework compatibility. |
| `LongCount` | Same as `Count` but returns `long`. |
| `GroupBy` | Key selector + optional element selector. Compiled as `Enumerable.GroupBy`. |
| `Cast` | Pass-through (no transformation needed). |

### ⏳ Pending implementation (currently falls back to client evaluation)

These operators return `null` from the translator, which causes EF Core to evaluate them on the client side after materialization. They are candidates for future server-side translation.

| Operator | Feasibility | Notes |
|---|---|---|
| `Skip` | 🟢 High | Straightforward — chain `Enumerable.Skip`. |
| `Take` | 🟢 High | Straightforward — chain `Enumerable.Take`. |
| `Distinct` | 🟢 High | Chain `Enumerable.Distinct`. |
| `Any` | 🟢 High | Can be implemented similarly to `Count` (check > 0). |
| `All` | 🟢 High | Negate + Any pattern. |
| `Contains` | 🟡 Medium | Depends on expression type of the item. |
| `Min` / `Max` | 🟡 Medium | Requires selector handling + nullable result types. |
| `Sum` / `Average` | 🟡 Medium | Requires numeric type coercion. |
| `ElementAtOrDefault` | 🟡 Medium | Skip(n).Take(1) pattern. |
| `DefaultIfEmpty` | 🟡 Medium | Append default element if empty. |
| `OfType` | 🟡 Medium | Requires type hierarchy support. |
| `Reverse` | 🟡 Medium | Chain `Enumerable.Reverse`. |
| `Concat` | 🔴 Low | Cross-query combination. |
| `Union` | 🔴 Low | Cross-query set operation. |
| `Intersect` | 🔴 Low | Cross-query set operation. |
| `Except` | 🔴 Low | Cross-query set operation. |
| `SkipWhile` | 🔴 Low | Not common in EF Core queries. |
| `TakeWhile` | 🔴 Low | Not common in EF Core queries. |

### 🚫 Not supported (by design)

LiteDB is a document database — these operations involve cross-collection relationships that have no server-side equivalent. They throw `InvalidOperationException` with an explicit message, or fall back to client evaluation.

| Operator | Behavior | Workaround |
|---|---|---|
| `Join` | Returns `null` (client eval) | Use `.AsEnumerable()` before `.Join(...)` |
| `LeftJoin` | Returns `null` (client eval) | Use `.AsEnumerable()` before `.LeftJoin(...)` |
| `RightJoin` | Returns `null` (client eval) | Use `.AsEnumerable()` before `.RightJoin(...)` |
| `GroupJoin` | ❌ Throws `InvalidOperationException` | Use `.AsEnumerable()` before `.GroupJoin(...)` |
| `SelectMany` | ❌ Throws `InvalidOperationException` | Use `.AsEnumerable()` before `.SelectMany(...)` |

> **Why do `GroupJoin` and `SelectMany` throw instead of falling back?**
> Unlike `Join`, these operators are commonly misused expecting server-side cross-collection behavior. An explicit exception provides clearer guidance than a silent client-side evaluation that could hide performance problems on large datasets.

## Getting started

Build and run tests:

```pwsh
dotnet build
dotnet test
```

Example `DbContext` usage:

```csharp
public class TestDbContext : DbContext
{
    private readonly string _dbPath;
    public TestDbContext(string dbPath) => _dbPath = dbPath;

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseLiteDb(_dbPath);
}
```

Cross-collection joins (client-side):

```csharp
var results = context.Orders.AsEnumerable()
    .Join(
        context.Products.AsEnumerable(),
        o => o.ProductName,
        p => p.Name,
        (order, product) => new { order.Id, product.Price });
```

## Project layout

```
NpgLitedb/
├── Extensions/           # UseLiteDb() and service registration
├── Infrastructure/       # Options extension
├── Storage/              # IDatabase, connection, creator, transactions, type mapping
├── Query/Internal/       # Query pipeline (expression, translator, compiler)
├── Metadata/             # Conventions and model validation
├── ValueGeneration/      # Value generator selector
└── Diagnostics/          # Logging definitions

Test/
├── DAL/                  # TestDbContext
├── Entity/               # Customer, Product, Order
├── LiteDbTestBase.cs     # Shared base class
└── LiteDb*Tests.cs       # 88 integration tests (9 test classes)
```

## Architecture

The query pipeline works as follows:

1. **Translation** — `LiteDbQueryableMethodTranslatingExpressionVisitor` intercepts LINQ method calls and stores operations (predicates, orderings, selectors, etc.) on a `LiteDbQueryExpression`.
2. **Compilation** — `LiteDbShapedQueryCompilingExpressionVisitor` builds an expression tree that chains `Enumerable` methods (`Where`, `OrderBy`, `Select`, `GroupBy`, `Take`, etc.) over the materialized entities.
3. **Execution** — The compiled delegate runs against `BsonDocument` rows fetched from LiteDB via `LiteCollection.FindAll()`, materializes entities, and integrates with EF Core's change tracker.

## Development notes

- The provider registers core EF services via `EntityFrameworkServicesBuilder` and calls `.TryAddCoreServices()` to ensure EF internals are available.
- Query execution materializes entities from `LiteDB.BsonDocument` instances; see `LiteDbShapedQueryCompilingExpressionVisitor.cs`.
- Scalar results (`Count`, `LongCount`) are wrapped in `NewArrayInit` to satisfy EF Core's `GetSequenceType()` requirement.
- Element operators (`First`, `Last`, etc.) use `Take(1)` / `TakeLast(1)` because the framework always wraps results with `Single()` / `SingleOrDefault()`.

## Test coverage

| Test class | Tests | Scope |
|---|---|---|
| `LiteDbProviderBasicTests` | 10 | CRUD, EnsureCreated/Deleted, multi-entity |
| `LiteDbWhereTests` | 11 | Filtering: string, numeric, bool, decimal, compound, negation, chaining |
| `LiteDbSelectTests` | 10 | Projections: anonymous, single prop, computations, bool, empty |
| `LiteDbOrderByTests` | 10 | Ordering: asc/desc, ThenBy, bool, decimal, with Where/Select |
| `LiteDbCountTests` | 8 | Count/LongCount: all, predicate, Where+predicate, empty |
| `LiteDbJoinTests` | 6 | Client-side Join via AsEnumerable: basic, empty, no-match, multi-match |
| `LiteDbElementTests` | 23 | First/Single/Last + OrDefault: predicate, empty, throws |
| `LiteDbGroupByTests` | 10 | GroupBy: key types, element access, empty, combined with Where/OrderBy |
| **Total** | **88** | |

## Contributing

Contributions welcome. Please add focused changes and tests. This project is experimental — open issues or PRs for improvements.

## License

MIT (see repository LICENSE if present).
