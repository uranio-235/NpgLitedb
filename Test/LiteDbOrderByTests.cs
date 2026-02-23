using Test.Entity;

namespace Test;

[TestClass]
public sealed class LiteDbOrderByTests : LiteDbTestBase
{
    [TestMethod]
    public void OrderBy_AscendingByName()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Charlie", Age = 45, IsActive = true },
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = true }
        );
        context.SaveChanges();

        var result = context.Customers.OrderBy(c => c.Name).ToList();

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Alice", result[0].Name);
        Assert.AreEqual("Bob", result[1].Name);
        Assert.AreEqual("Charlie", result[2].Name);
    }

    [TestMethod]
    public void OrderByDescending_ByAge()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Young", Age = 20, IsActive = true },
            new Customer { Name = "Old", Age = 60, IsActive = true },
            new Customer { Name = "Middle", Age = 40, IsActive = true }
        );
        context.SaveChanges();

        var result = context.Customers.OrderByDescending(c => c.Age).ToList();

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(60, result[0].Age);
        Assert.AreEqual(40, result[1].Age);
        Assert.AreEqual(20, result[2].Age);
    }

    [TestMethod]
    public void OrderBy_ThenBy()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Bob", Age = 30, IsActive = true },
            new Customer { Name = "Alice", Age = 40, IsActive = true },
            new Customer { Name = "Bob", Age = 20, IsActive = true },
            new Customer { Name = "Alice", Age = 25, IsActive = true }
        );
        context.SaveChanges();

        var result = context.Customers
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Age)
            .ToList();

        Assert.AreEqual(4, result.Count);
        Assert.AreEqual("Alice", result[0].Name);
        Assert.AreEqual(25, result[0].Age);
        Assert.AreEqual("Alice", result[1].Name);
        Assert.AreEqual(40, result[1].Age);
        Assert.AreEqual("Bob", result[2].Name);
        Assert.AreEqual(20, result[2].Age);
        Assert.AreEqual("Bob", result[3].Name);
        Assert.AreEqual(30, result[3].Age);
    }

    [TestMethod]
    public void OrderBy_ThenByDescending()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Bob", Age = 30, IsActive = true },
            new Customer { Name = "Alice", Age = 40, IsActive = true },
            new Customer { Name = "Bob", Age = 20, IsActive = true },
            new Customer { Name = "Alice", Age = 25, IsActive = true }
        );
        context.SaveChanges();

        var result = context.Customers
            .OrderBy(c => c.Name)
            .ThenByDescending(c => c.Age)
            .ToList();

        Assert.AreEqual(4, result.Count);
        Assert.AreEqual("Alice", result[0].Name);
        Assert.AreEqual(40, result[0].Age);
        Assert.AreEqual("Alice", result[1].Name);
        Assert.AreEqual(25, result[1].Age);
        Assert.AreEqual("Bob", result[2].Name);
        Assert.AreEqual(30, result[2].Age);
        Assert.AreEqual("Bob", result[3].Name);
        Assert.AreEqual(20, result[3].Age);
    }

    [TestMethod]
    public void OrderBy_ByDecimalProperty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Products.AddRange(
            new Product { Name = "Mid", Price = 50.00m, Stock = 10 },
            new Product { Name = "Cheap", Price = 5.00m, Stock = 100 },
            new Product { Name = "Expensive", Price = 500.00m, Stock = 1 }
        );
        context.SaveChanges();

        var result = context.Products.OrderBy(p => p.Price).ToList();

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Cheap", result[0].Name);
        Assert.AreEqual("Mid", result[1].Name);
        Assert.AreEqual("Expensive", result[2].Name);
    }

    [TestMethod]
    public void OrderBy_ByBoolProperty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Active1", Age = 25, IsActive = true },
            new Customer { Name = "Inactive", Age = 35, IsActive = false },
            new Customer { Name = "Active2", Age = 45, IsActive = true }
        );
        context.SaveChanges();

        // false (0) comes before true (1)
        var result = context.Customers.OrderBy(c => c.IsActive).ToList();

        Assert.AreEqual(3, result.Count);
        Assert.IsFalse(result[0].IsActive);
        Assert.IsTrue(result[1].IsActive);
        Assert.IsTrue(result[2].IsActive);
    }

    [TestMethod]
    public void OrderBy_WithWhere()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Charlie", Age = 45, IsActive = true },
            new Customer { Name = "Alice", Age = 25, IsActive = false },
            new Customer { Name = "Bob", Age = 35, IsActive = true },
            new Customer { Name = "Diana", Age = 28, IsActive = true }
        );
        context.SaveChanges();

        var result = context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToList();

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Bob", result[0].Name);
        Assert.AreEqual("Charlie", result[1].Name);
        Assert.AreEqual("Diana", result[2].Name);
    }

    [TestMethod]
    public void OrderBy_WithSelect()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Charlie", Age = 45, IsActive = true },
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = true }
        );
        context.SaveChanges();

        var result = context.Customers
            .OrderBy(c => c.Age)
            .Select(c => c.Name)
            .ToList();

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Alice", result[0]);
        Assert.AreEqual("Bob", result[1]);
        Assert.AreEqual("Charlie", result[2]);
    }

    [TestMethod]
    public void OrderBy_EmptyCollection()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        var result = context.Customers.OrderBy(c => c.Name).ToList();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void OrderBy_SingleElement()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Only", Age = 30, IsActive = true });
        context.SaveChanges();

        var result = context.Customers.OrderBy(c => c.Name).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Only", result[0].Name);
    }
}
