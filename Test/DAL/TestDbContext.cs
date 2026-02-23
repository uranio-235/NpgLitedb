using Microsoft.EntityFrameworkCore;
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLiteDb(_dbPath);
    }
}
