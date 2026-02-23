using Test.Entity;

namespace Test;

/// <summary>
/// Tests for GroupBy LINQ operations.
/// </summary>
[TestClass]
public sealed class LiteDbGroupByTests : LiteDbTestBase
{
    [TestMethod]
    public void GroupBy_SimpleKeySelector()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 25, IsActive = false },
            new Customer { Name = "Charlie", Age = 35, IsActive = true }
        );
        context.SaveChanges();

        var groups = context.Customers
            .GroupBy(c => c.Age)
            .ToList();

        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(2, groups.First(g => g.Key == 25).Count());
        Assert.AreEqual(1, groups.First(g => g.Key == 35).Count());
    }

    [TestMethod]
    public void GroupBy_StringKey()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Alice", Age = 30, IsActive = false },
            new Customer { Name = "Bob", Age = 35, IsActive = true }
        );
        context.SaveChanges();

        var groups = context.Customers
            .GroupBy(c => c.Name)
            .ToList();

        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(2, groups.First(g => g.Key == "Alice").Count());
        Assert.AreEqual(1, groups.First(g => g.Key == "Bob").Count());
    }

    [TestMethod]
    public void GroupBy_BoolKey()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true }
        );
        context.SaveChanges();

        var groups = context.Customers
            .GroupBy(c => c.IsActive)
            .ToList();

        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(2, groups.First(g => g.Key).Count());
        Assert.AreEqual(1, groups.First(g => !g.Key).Count());
    }

    [TestMethod]
    public void GroupBy_EmptyCollection()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        var groups = context.Customers
            .GroupBy(c => c.Age)
            .ToList();

        Assert.AreEqual(0, groups.Count);
    }

    [TestMethod]
    public void GroupBy_SingleGroup()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 25, IsActive = false }
        );
        context.SaveChanges();

        var groups = context.Customers
            .GroupBy(c => c.Age)
            .ToList();

        Assert.AreEqual(1, groups.Count);
        Assert.AreEqual(25, groups[0].Key);
        Assert.AreEqual(2, groups[0].Count());
    }

    [TestMethod]
    public void GroupBy_EachElementOwnGroup()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true }
        );
        context.SaveChanges();

        var groups = context.Customers
            .GroupBy(c => c.Age)
            .ToList();

        Assert.AreEqual(3, groups.Count);
        Assert.IsTrue(groups.All(g => g.Count() == 1));
    }

    [TestMethod]
    public void GroupBy_WithWhereBeforeGroupBy()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 25, IsActive = false },
            new Customer { Name = "Charlie", Age = 35, IsActive = true },
            new Customer { Name = "Diana", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var groups = context.Customers
            .Where(c => c.IsActive)
            .GroupBy(c => c.Age)
            .ToList();

        Assert.AreEqual(2, groups.Count);
        Assert.IsTrue(groups.All(g => g.Count() == 1));
        Assert.IsTrue(groups.All(g => g.All(c => c.IsActive)));
    }

    [TestMethod]
    public void GroupBy_AccessGroupElements()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 25, IsActive = false },
            new Customer { Name = "Charlie", Age = 35, IsActive = true }
        );
        context.SaveChanges();

        var groups = context.Customers
            .GroupBy(c => c.Age)
            .ToList();

        var age25Group = groups.First(g => g.Key == 25);
        var names = age25Group.Select(c => c.Name).OrderBy(n => n).ToList();

        Assert.AreEqual(2, names.Count);
        Assert.AreEqual("Alice", names[0]);
        Assert.AreEqual("Bob", names[1]);
    }

    [TestMethod]
    public void GroupBy_WithOrderByBeforeGroupBy()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Charlie", Age = 25, IsActive = true },
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var groups = context.Customers
            .OrderBy(c => c.Name)
            .GroupBy(c => c.Age)
            .ToList();

        Assert.AreEqual(2, groups.Count);

        var age25Group = groups.First(g => g.Key == 25);
        Assert.AreEqual("Alice", age25Group.First().Name);
    }

    [TestMethod]
    public void GroupBy_ProductsByPrice()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Products.AddRange(
            new Product { Name = "Widget", Price = 10.00m, Stock = 5 },
            new Product { Name = "Gadget", Price = 10.00m, Stock = 3 },
            new Product { Name = "Doohickey", Price = 20.00m, Stock = 8 }
        );
        context.SaveChanges();

        var groups = context.Products
            .GroupBy(p => p.Price)
            .ToList();

        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(2, groups.First(g => g.Key == 10.00m).Count());
        Assert.AreEqual(1, groups.First(g => g.Key == 20.00m).Count());
    }
}
