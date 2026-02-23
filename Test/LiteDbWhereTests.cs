using Test.Entity;

namespace Test;

[TestClass]
public sealed class LiteDbWhereTests : LiteDbTestBase
{
    [TestMethod]
    public void Where_FiltersByBoolProperty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true }
        );
        context.SaveChanges();

        var active = context.Customers.Where(c => c.IsActive).ToList();

        Assert.AreEqual(2, active.Count);
        Assert.IsTrue(active.All(c => c.IsActive));
    }

    [TestMethod]
    public void Where_FiltersByStringEquality()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var result = context.Customers.Where(c => c.Name == "Alice").ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Alice", result[0].Name);
    }

    [TestMethod]
    public void Where_FiltersByNumericComparison()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Young", Age = 20, IsActive = true },
            new Customer { Name = "Middle", Age = 35, IsActive = true },
            new Customer { Name = "Senior", Age = 60, IsActive = true }
        );
        context.SaveChanges();

        var over30 = context.Customers.Where(c => c.Age > 30).ToList();

        Assert.AreEqual(2, over30.Count);
        Assert.IsTrue(over30.All(c => c.Age > 30));
    }

    [TestMethod]
    public void Where_ChainedFilters()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = true },
            new Customer { Name = "Charlie", Age = 45, IsActive = false },
            new Customer { Name = "Diana", Age = 28, IsActive = true }
        );
        context.SaveChanges();

        var result = context.Customers
            .Where(c => c.IsActive)
            .Where(c => c.Age >= 30)
            .ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Bob", result[0].Name);
    }

    [TestMethod]
    public void Where_CompoundConditionWithAnd()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true }
        );
        context.SaveChanges();

        var result = context.Customers
            .Where(c => c.IsActive && c.Age > 30)
            .ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Charlie", result[0].Name);
    }

    [TestMethod]
    public void Where_CompoundConditionWithOr()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true }
        );
        context.SaveChanges();

        var result = context.Customers
            .Where(c => c.Name == "Alice" || c.Age > 40)
            .ToList();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void Where_ReturnsEmptyWhenNoMatch()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Alice", Age = 25, IsActive = true });
        context.SaveChanges();

        var result = context.Customers.Where(c => c.Name == "NonExistent").ToList();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Where_NegatedCondition()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Active1", Age = 20, IsActive = true },
            new Customer { Name = "Inactive1", Age = 30, IsActive = false },
            new Customer { Name = "Active2", Age = 40, IsActive = true }
        );
        context.SaveChanges();

        var inactive = context.Customers.Where(c => !c.IsActive).ToList();

        Assert.AreEqual(1, inactive.Count);
        Assert.AreEqual("Inactive1", inactive[0].Name);
    }

    [TestMethod]
    public void Where_OnDecimalProperty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Products.AddRange(
            new Product { Name = "Cheap", Price = 5.99m, Stock = 100 },
            new Product { Name = "Mid", Price = 49.99m, Stock = 50 },
            new Product { Name = "Expensive", Price = 199.99m, Stock = 10 }
        );
        context.SaveChanges();

        var expensive = context.Products.Where(p => p.Price > 50m).ToList();

        Assert.AreEqual(1, expensive.Count);
        Assert.AreEqual("Expensive", expensive[0].Name);
    }

    [TestMethod]
    public void Where_WithTrackedEntitiesInSameContext()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        // Query with filter in the same context where entities are already tracked
        var active = context.Customers.Where(c => c.IsActive).ToList();

        Assert.AreEqual(1, active.Count);
        Assert.AreEqual("Alice", active[0].Name);
    }

    [TestMethod]
    public void Where_FilterThenUpdateThenVerify()
    {
        using (var context = CreateContext())
        {
            context.Database.EnsureCreated();

            context.Customers.AddRange(
                new Customer { Name = "Alice", Age = 25, IsActive = true },
                new Customer { Name = "Bob", Age = 35, IsActive = true }
            );
            context.SaveChanges();
        }

        // Update a filtered entity
        using (var context = CreateContext())
        {
            var old = context.Customers.Where(c => c.Age > 30).ToList();
            Assert.AreEqual(1, old.Count);

            old[0].IsActive = false;
            context.SaveChanges();
        }

        // Verify the change persisted
        using (var context = CreateContext())
        {
            var stillActive = context.Customers.Where(c => c.IsActive).ToList();
            Assert.AreEqual(1, stillActive.Count);
            Assert.AreEqual("Alice", stillActive[0].Name);
        }
    }
}
