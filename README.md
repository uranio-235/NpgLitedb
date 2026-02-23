# NpgLitedb — Entity Framework Core provider for LiteDB (experimental)

Lightweight EF Core provider enabling LiteDB as a backing store for Entity Framework Core. This project is experimental and intended for learning and prototyping.

Status: Build successful and tests passing in this workspace (10/10 passing).

## Overview

`NpgLitedb` implements a minimal EF Core provider surface that allows using LiteDB (single-file, serverless NoSQL) with EF Core 10. It implements provider registration, a basic `IDatabase` implementation, query compilation hooks, type mapping, and a materialization pipeline.

Key points:
- Targets .NET 10 and EF Core 10
- Uses `LiteDB` as the persistence engine
- Focused on CRUD and simple query scenarios

## Features

- Configure `DbContext` with `UseLiteDb(path)`
- `EnsureCreated` / `EnsureDeleted` support
- CRUD via `DbSet<T>` and `SaveChanges()` / `SaveChangesAsync()`
- Basic LINQ query support with entity materialization and change-tracking integration

Limitations:
- Not a relational provider — no SQL, no migrations, and limited query translation. Many LINQ operators are evaluated client-side.

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

## Project layout

- `NpgLitedb/` — provider implementation
- `Test/` — integration-style tests demonstrating basic workflows

## Development notes

- The provider registers core EF services via `EntityFrameworkServicesBuilder` and calls `.TryAddCoreServices()` to ensure EF internals are available.
- Query execution is implemented by materializing entities from `LiteDB.BsonDocument` instances; see `NpgLitedb/Query/Internal/LiteDbShapedQueryCompilingExpressionVisitor.cs`.

## Contributing

Contributions welcome. Please add focused changes and tests. This project is experimental — open issues or PRs for improvements.

## License

MIT (see repository LICENSE if present).
