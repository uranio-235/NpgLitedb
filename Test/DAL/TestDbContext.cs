using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NpgLitedb.Extensions;
using Test.Entity;

namespace Test.DAL;

public class TestDbContext : DbContext
{
    private readonly string _dbPath;

    public TestDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLiteDb(_dbPath)
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
    }
}
