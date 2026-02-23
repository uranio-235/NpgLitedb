using Test.Entity;

namespace Test;

[TestClass]
public sealed class LiteDbCountTests : LiteDbTestBase
{
    [TestMethod]
    public void Count_AllEntities()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true }
        );
        context.SaveChanges();

        var count = context.Customers.Count();

        Assert.AreEqual(3, count);
    }

    [TestMethod]
    public void Count_WithPredicate()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true }
        );
        context.SaveChanges();

        var count = context.Customers.Count(c => c.IsActive);

        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public void Count_EmptyCollection()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        var count = context.Customers.Count();

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void Count_AfterWhere()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Young1", Age = 20, IsActive = true },
            new Customer { Name = "Young2", Age = 25, IsActive = true },
            new Customer { Name = "Old1", Age = 50, IsActive = true },
            new Customer { Name = "Old2", Age = 60, IsActive = true }
        );
        context.SaveChanges();

        var count = context.Customers.Where(c => c.Age >= 50).Count();

        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public void Count_WhereAndPredicate()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true },
            new Customer { Name = "Diana", Age = 55, IsActive = false }
        );
        context.SaveChanges();

        var count = context.Customers
            .Where(c => c.Age > 30)
            .Count(c => c.IsActive);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void Count_OnProducts()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Products.AddRange(
            new Product { Name = "Cheap", Price = 5.00m, Stock = 100 },
            new Product { Name = "Expensive", Price = 500.00m, Stock = 1 }
        );
        context.SaveChanges();

        var count = context.Products.Count(p => p.Price > 100m);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void LongCount_AllEntities()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var count = context.Customers.LongCount();

        Assert.AreEqual(2L, count);
    }

    [TestMethod]
    public void LongCount_WithPredicate()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true }
        );
        context.SaveChanges();

        var count = context.Customers.LongCount(c => !c.IsActive);

        Assert.AreEqual(1L, count);
    }
}
