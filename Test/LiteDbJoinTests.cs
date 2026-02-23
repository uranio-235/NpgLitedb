using Test.Entity;

namespace Test;

/// <summary>
/// Tests for cross-collection joins using client-side evaluation.
/// LiteDB is a document database, so joins are performed after materializing both collections.
/// </summary>
[TestClass]
public sealed class LiteDbJoinTests : LiteDbTestBase
{
    [TestMethod]
    public void Join_BasicInnerJoin()
    {
        int aliceId, bobId;

        using (var context = CreateContext())
        {
            context.Database.EnsureCreated();

            context.Customers.AddRange(
                new Customer { Name = "Alice", Age = 25, IsActive = true },
                new Customer { Name = "Bob", Age = 35, IsActive = true }
            );
            context.SaveChanges();

            var customers = context.Customers.OrderBy(c => c.Name).ToList();
            aliceId = customers[0].Id;
            bobId = customers[1].Id;

            context.Orders.AddRange(
                new Order { CustomerId = aliceId, ProductName = "Widget", Amount = 9.99m },
                new Order { CustomerId = aliceId, ProductName = "Gadget", Amount = 19.99m },
                new Order { CustomerId = bobId, ProductName = "Doohickey", Amount = 4.99m }
            );
            context.SaveChanges();
        }

        using (var context = CreateContext())
        {
            var result = context.Customers.AsEnumerable()
                .Join(context.Orders.AsEnumerable(),
                    c => c.Id,
                    o => o.CustomerId,
                    (c, o) => new { CustomerName = c.Name, o.ProductName, o.Amount })
                .ToList();

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result.Count(r => r.CustomerName == "Alice"));
            Assert.AreEqual(1, result.Count(r => r.CustomerName == "Bob"));
        }
    }

    [TestMethod]
    public void Join_NoMatchingKeys()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Alice", Age = 25, IsActive = true });
        context.Orders.Add(new Order { CustomerId = 999, ProductName = "Widget", Amount = 9.99m });
        context.SaveChanges();

        var result = context.Customers.AsEnumerable()
            .Join(context.Orders.AsEnumerable(),
                c => c.Id,
                o => o.CustomerId,
                (c, o) => new { c.Name, o.ProductName })
            .ToList();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Join_EmptyOuterCollection()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Orders.Add(new Order { CustomerId = 1, ProductName = "Widget", Amount = 9.99m });
        context.SaveChanges();

        var result = context.Customers.AsEnumerable()
            .Join(context.Orders.AsEnumerable(),
                c => c.Id,
                o => o.CustomerId,
                (c, o) => new { c.Name, o.ProductName })
            .ToList();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Join_EmptyInnerCollection()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Alice", Age = 25, IsActive = true });
        context.SaveChanges();

        var result = context.Customers.AsEnumerable()
            .Join(context.Orders.AsEnumerable(),
                c => c.Id,
                o => o.CustomerId,
                (c, o) => new { c.Name, o.ProductName })
            .ToList();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Join_MultipleMatchesPerKey()
    {
        int aliceId;

        using (var context = CreateContext())
        {
            context.Database.EnsureCreated();

            context.Customers.Add(new Customer { Name = "Alice", Age = 25, IsActive = true });
            context.SaveChanges();
            aliceId = context.Customers.First().Id;

            context.Orders.AddRange(
                new Order { CustomerId = aliceId, ProductName = "Widget", Amount = 10m },
                new Order { CustomerId = aliceId, ProductName = "Gadget", Amount = 20m },
                new Order { CustomerId = aliceId, ProductName = "Thingamajig", Amount = 30m }
            );
            context.SaveChanges();
        }

        using (var context = CreateContext())
        {
            var result = context.Customers.AsEnumerable()
                .Join(context.Orders.AsEnumerable(),
                    c => c.Id,
                    o => o.CustomerId,
                    (c, o) => new { c.Name, o.ProductName })
                .ToList();

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.All(r => r.Name == "Alice"));
        }
    }

    [TestMethod]
    public void Join_WithWhereOnOuterCollection()
    {
        using (var context = CreateContext())
        {
            context.Database.EnsureCreated();

            context.Customers.AddRange(
                new Customer { Name = "Alice", Age = 25, IsActive = true },
                new Customer { Name = "Bob", Age = 35, IsActive = false }
            );
            context.SaveChanges();

            var customers = context.Customers.OrderBy(c => c.Name).ToList();
            var aliceId = customers[0].Id;
            var bobId = customers[1].Id;

            context.Orders.AddRange(
                new Order { CustomerId = aliceId, ProductName = "Widget", Amount = 10m },
                new Order { CustomerId = bobId, ProductName = "Gadget", Amount = 20m }
            );
            context.SaveChanges();
        }

        using (var context = CreateContext())
        {
            var result = context.Customers.Where(c => c.IsActive).AsEnumerable()
                .Join(context.Orders.AsEnumerable(),
                    c => c.Id,
                    o => o.CustomerId,
                    (c, o) => new { c.Name, o.ProductName })
                .ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Alice", result[0].Name);
            Assert.AreEqual("Widget", result[0].ProductName);
        }
    }
}

