using LiteDB;
using Microsoft.EntityFrameworkCore;
using NpgLitedb.Extensions;

namespace Test;

#region Test Entities

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

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

#endregion

[TestClass]
public sealed class LiteDbProviderBasicTests
{
    private string _dbPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"litedb_test_{Guid.NewGuid():N}.db");
    }

    [TestCleanup]
    public void Cleanup()
    {
        try { File.Delete(_dbPath); } catch { }
        try { File.Delete(_dbPath + "-log"); } catch { }
    }

    [TestMethod]
    public void CanCreateDatabase()
    {
        using var context = new TestDbContext(_dbPath);
        var created = context.Database.EnsureCreated();
        Assert.IsTrue(created);
        Assert.IsTrue(File.Exists(_dbPath));
    }

    [TestMethod]
    public void CanInsertAndRetrieveEntity()
    {
        using (var context = new TestDbContext(_dbPath))
        {
            context.Database.EnsureCreated();

            var customer = new Customer
            {
                Name = "John Doe",
                Age = 30,
                IsActive = true
            };

            context.Customers.Add(customer);
            var saved = context.SaveChanges();

            Assert.AreEqual(1, saved);
        }

        // Read back in a new context
        using (var context = new TestDbContext(_dbPath))
        {
            var customers = context.Customers.ToList();

            Assert.AreEqual(1, customers.Count);
            Assert.AreEqual("John Doe", customers[0].Name);
            Assert.AreEqual(30, customers[0].Age);
            Assert.IsTrue(customers[0].IsActive);
        }
    }

    [TestMethod]
    public void CanInsertMultipleEntities()
    {
        using var context = new TestDbContext(_dbPath);
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true }
        );

        var saved = context.SaveChanges();
        Assert.AreEqual(3, saved);

        var allCustomers = context.Customers.ToList();
        Assert.AreEqual(3, allCustomers.Count);
    }

    [TestMethod]
    public void CanUpdateEntity()
    {
        using (var context = new TestDbContext(_dbPath))
        {
            context.Database.EnsureCreated();

            var customer = new Customer { Name = "Original", Age = 20, IsActive = true };
            context.Customers.Add(customer);
            context.SaveChanges();
        }

        using (var context = new TestDbContext(_dbPath))
        {
            var customer = context.Customers.ToList().First();
            customer.Name = "Updated";
            customer.Age = 21;
            context.SaveChanges();
        }

        using (var context = new TestDbContext(_dbPath))
        {
            var customer = context.Customers.ToList().First();
            Assert.AreEqual("Updated", customer.Name);
            Assert.AreEqual(21, customer.Age);
        }
    }

    [TestMethod]
    public void CanDeleteEntity()
    {
        using (var context = new TestDbContext(_dbPath))
        {
            context.Database.EnsureCreated();

            context.Customers.AddRange(
                new Customer { Name = "Keep", Age = 30, IsActive = true },
                new Customer { Name = "Delete", Age = 40, IsActive = false }
            );
            context.SaveChanges();
        }

        using (var context = new TestDbContext(_dbPath))
        {
            var toDelete = context.Customers.ToList().First(c => c.Name == "Delete");
            context.Customers.Remove(toDelete);
            var deleted = context.SaveChanges();
            Assert.AreEqual(1, deleted);
        }

        using (var context = new TestDbContext(_dbPath))
        {
            var remaining = context.Customers.ToList();
            Assert.AreEqual(1, remaining.Count);
            Assert.AreEqual("Keep", remaining[0].Name);
        }
    }

    [TestMethod]
    public void CanWorkWithMultipleEntityTypes()
    {
        using var context = new TestDbContext(_dbPath);
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Buyer", Age = 28, IsActive = true });
        context.Products.Add(new Product { Name = "Widget", Price = 9.99m, Stock = 100 });
        context.SaveChanges();

        var customers = context.Customers.ToList();
        var products = context.Products.ToList();

        Assert.AreEqual(1, customers.Count);
        Assert.AreEqual(1, products.Count);
        Assert.AreEqual("Widget", products[0].Name);
        Assert.AreEqual(9.99m, products[0].Price);
    }

    [TestMethod]
    public void EnsureDeletedDropsAllCollections()
    {
        using (var context = new TestDbContext(_dbPath))
        {
            context.Database.EnsureCreated();
            context.Customers.Add(new Customer { Name = "Test", Age = 1, IsActive = true });
            context.SaveChanges();
        }

        using (var context = new TestDbContext(_dbPath))
        {
            context.Database.EnsureDeleted();
        }

        using (var context = new TestDbContext(_dbPath))
        {
            var customers = context.Customers.ToList();
            Assert.AreEqual(0, customers.Count);
        }
    }

    [TestMethod]
    public void CanConnectReturnsTrue()
    {
        using var context = new TestDbContext(_dbPath);
        Assert.IsTrue(context.Database.CanConnect());
    }

    [TestMethod]
    public async Task CanInsertAndRetrieveAsync()
    {
        await using (var context = new TestDbContext(_dbPath))
        {
            await context.Database.EnsureCreatedAsync();

            context.Customers.Add(new Customer { Name = "Async User", Age = 22, IsActive = true });
            var saved = await context.SaveChangesAsync();
            Assert.AreEqual(1, saved);
        }

        await using (var context = new TestDbContext(_dbPath))
        {
            var customers = context.Customers.ToList();
            Assert.AreEqual(1, customers.Count);
            Assert.AreEqual("Async User", customers[0].Name);
        }
    }

    [TestMethod]
    public void CanHandleDecimalPrecision()
    {
        using var context = new TestDbContext(_dbPath);
        context.Database.EnsureCreated();

        context.Products.Add(new Product { Name = "Precise", Price = 123456.789m, Stock = 1 });
        context.SaveChanges();

        var product = context.Products.ToList().First();
        Assert.AreEqual(123456.789m, product.Price);
    }
}
