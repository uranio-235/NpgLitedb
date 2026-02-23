using Test.Entity;

namespace Test;

[TestClass]
public sealed class LiteDbSelectTests : LiteDbTestBase
{
    [TestMethod]
    public void Select_SingleProperty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var names = context.Customers.Select(c => c.Name).ToList();

        Assert.AreEqual(2, names.Count);
        CollectionAssert.Contains(names, "Alice");
        CollectionAssert.Contains(names, "Bob");
    }

    [TestMethod]
    public void Select_AnonymousType()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var result = context.Customers
            .Select(c => new { c.Name, c.Age })
            .ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Alice", result.First(r => r.Age == 25).Name);
        Assert.AreEqual("Bob", result.First(r => r.Age == 35).Name);
    }

    [TestMethod]
    public void Select_WithComputation()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Products.AddRange(
            new Product { Name = "Widget", Price = 10.00m, Stock = 5 },
            new Product { Name = "Gadget", Price = 20.00m, Stock = 3 }
        );
        context.SaveChanges();

        var totals = context.Products
            .Select(p => new { p.Name, Total = p.Price * p.Stock })
            .ToList();

        Assert.AreEqual(2, totals.Count);
        Assert.AreEqual(50.00m, totals.First(t => t.Name == "Widget").Total);
        Assert.AreEqual(60.00m, totals.First(t => t.Name == "Gadget").Total);
    }

    [TestMethod]
    public void Select_WhereBeforeSelect()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true }
        );
        context.SaveChanges();

        var activeNames = context.Customers
            .Where(c => c.IsActive)
            .Select(c => c.Name)
            .ToList();

        Assert.AreEqual(2, activeNames.Count);
        CollectionAssert.Contains(activeNames, "Alice");
        CollectionAssert.Contains(activeNames, "Charlie");
        CollectionAssert.DoesNotContain(activeNames, "Bob");
    }

    [TestMethod]
    public void Select_NumericProperty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Young", Age = 20, IsActive = true },
            new Customer { Name = "Old", Age = 60, IsActive = true }
        );
        context.SaveChanges();

        var ages = context.Customers.Select(c => c.Age).ToList();

        Assert.AreEqual(2, ages.Count);
        CollectionAssert.Contains(ages, 20);
        CollectionAssert.Contains(ages, 60);
    }

    [TestMethod]
    public void Select_BoolProperty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Active", Age = 30, IsActive = true },
            new Customer { Name = "Inactive", Age = 40, IsActive = false }
        );
        context.SaveChanges();

        var flags = context.Customers.Select(c => c.IsActive).ToList();

        Assert.AreEqual(2, flags.Count);
        Assert.AreEqual(1, flags.Count(f => f));
        Assert.AreEqual(1, flags.Count(f => !f));
    }

    [TestMethod]
    public void Select_DecimalProperty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Products.AddRange(
            new Product { Name = "Cheap", Price = 1.99m, Stock = 10 },
            new Product { Name = "Expensive", Price = 999.99m, Stock = 1 }
        );
        context.SaveChanges();

        var prices = context.Products.Select(p => p.Price).ToList();

        Assert.AreEqual(2, prices.Count);
        CollectionAssert.Contains(prices, 1.99m);
        CollectionAssert.Contains(prices, 999.99m);
    }

    [TestMethod]
    public void Select_ReturnsEmptyWhenSourceIsEmpty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        var names = context.Customers.Select(c => c.Name).ToList();

        Assert.AreEqual(0, names.Count);
    }

    [TestMethod]
    public void Select_WhereFiltersThenProjectsCorrectly()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Products.AddRange(
            new Product { Name = "Cheap", Price = 5.00m, Stock = 100 },
            new Product { Name = "Mid", Price = 50.00m, Stock = 50 },
            new Product { Name = "Expensive", Price = 500.00m, Stock = 5 }
        );
        context.SaveChanges();

        var expensiveNames = context.Products
            .Where(p => p.Price > 40m)
            .Select(p => new { p.Name, p.Price })
            .ToList();

        Assert.AreEqual(2, expensiveNames.Count);
        Assert.IsTrue(expensiveNames.All(x => x.Price > 40m));
        CollectionAssert.Contains(expensiveNames.Select(x => x.Name).ToList(), "Mid");
        CollectionAssert.Contains(expensiveNames.Select(x => x.Name).ToList(), "Expensive");
    }

    [TestMethod]
    public void Select_WithStringConcatenation()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var labels = context.Customers
            .Select(c => c.Name + " (Age: " + c.Age + ")")
            .ToList();

        Assert.AreEqual(2, labels.Count);
        CollectionAssert.Contains(labels, "Alice (Age: 25)");
        CollectionAssert.Contains(labels, "Bob (Age: 35)");
    }
}
