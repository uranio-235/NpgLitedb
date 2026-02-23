using Test.Entity;

namespace Test;

/// <summary>
/// Tests for First, FirstOrDefault, Single, SingleOrDefault, Last, LastOrDefault
/// — with and without predicates.
/// </summary>
[TestClass]
public sealed class LiteDbElementTests : LiteDbTestBase
{
    [TestMethod]
    public void First_ReturnsSingleEntity()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Alice", Age = 25, IsActive = true });
        context.SaveChanges();

        var customer = context.Customers.First();

        Assert.AreEqual("Alice", customer.Name);
    }

    [TestMethod]
    public void First_WithPredicate()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var customer = context.Customers.First(c => c.Age == 35);

        Assert.AreEqual("Bob", customer.Name);
    }

    [TestMethod]
    public void First_ThrowsWhenEmpty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => context.Customers.First());
    }

    [TestMethod]
    public void FirstOrDefault_ReturnsSingleEntity()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Alice", Age = 25, IsActive = true });
        context.SaveChanges();

        var customer = context.Customers.FirstOrDefault();

        Assert.IsNotNull(customer);
        Assert.AreEqual("Alice", customer.Name);
    }

    [TestMethod]
    public void FirstOrDefault_WithPredicate()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var customer = context.Customers.FirstOrDefault(c => !c.IsActive);

        Assert.IsNotNull(customer);
        Assert.AreEqual("Bob", customer.Name);
    }

    [TestMethod]
    public void FirstOrDefault_ReturnsNullWhenEmpty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        var customer = context.Customers.FirstOrDefault();

        Assert.IsNull(customer);
    }

    [TestMethod]
    public void FirstOrDefault_ReturnsNullWhenPredicateMatchesNothing()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Alice", Age = 25, IsActive = true });
        context.SaveChanges();

        var customer = context.Customers.FirstOrDefault(c => c.Age == 99);

        Assert.IsNull(customer);
    }

    [TestMethod]
    public void Single_ReturnsSingleEntity()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Alice", Age = 25, IsActive = true });
        context.SaveChanges();

        var customer = context.Customers.Single();

        Assert.AreEqual("Alice", customer.Name);
    }

    [TestMethod]
    public void Single_WithPredicate()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var customer = context.Customers.Single(c => c.Name == "Bob");

        Assert.AreEqual(35, customer.Age);
    }

    [TestMethod]
    public void Single_ThrowsWhenEmpty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => context.Customers.Single());
    }

    [TestMethod]
    public void Single_ThrowsWhenMultiple()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => context.Customers.Single());
    }

    [TestMethod]
    public void SingleOrDefault_ReturnsSingleEntity()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Alice", Age = 25, IsActive = true });
        context.SaveChanges();

        var customer = context.Customers.SingleOrDefault();

        Assert.IsNotNull(customer);
        Assert.AreEqual("Alice", customer.Name);
    }

    [TestMethod]
    public void SingleOrDefault_WithPredicate()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var customer = context.Customers.SingleOrDefault(c => c.Age == 25);

        Assert.IsNotNull(customer);
        Assert.AreEqual("Alice", customer.Name);
    }

    [TestMethod]
    public void SingleOrDefault_ReturnsNullWhenEmpty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        var customer = context.Customers.SingleOrDefault();

        Assert.IsNull(customer);
    }

    [TestMethod]
    public void SingleOrDefault_ThrowsWhenMultiple()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => context.Customers.SingleOrDefault());
    }

    [TestMethod]
    public void Last_ReturnsSingleEntity()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Alice", Age = 25, IsActive = true });
        context.SaveChanges();

        var customer = context.Customers.OrderBy(c => c.Name).Last();

        Assert.AreEqual("Alice", customer.Name);
    }

    [TestMethod]
    public void Last_WithPredicate()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = true },
            new Customer { Name = "Charlie", Age = 45, IsActive = false }
        );
        context.SaveChanges();

        var customer = context.Customers
            .OrderBy(c => c.Age)
            .Last(c => c.IsActive);

        Assert.AreEqual("Bob", customer.Name);
    }

    [TestMethod]
    public void Last_ThrowsWhenEmpty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => context.Customers.OrderBy(c => c.Name).Last());
    }

    [TestMethod]
    public void LastOrDefault_ReturnsSingleEntity()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.Add(new Customer { Name = "Alice", Age = 25, IsActive = true });
        context.SaveChanges();

        var customer = context.Customers.OrderBy(c => c.Name).LastOrDefault();

        Assert.IsNotNull(customer);
        Assert.AreEqual("Alice", customer.Name);
    }

    [TestMethod]
    public void LastOrDefault_WithPredicate()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = true },
            new Customer { Name = "Charlie", Age = 45, IsActive = false }
        );
        context.SaveChanges();

        var customer = context.Customers
            .OrderBy(c => c.Age)
            .LastOrDefault(c => c.IsActive);

        Assert.IsNotNull(customer);
        Assert.AreEqual("Bob", customer.Name);
    }

    [TestMethod]
    public void LastOrDefault_ReturnsNullWhenEmpty()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        var customer = context.Customers.OrderBy(c => c.Name).LastOrDefault();

        Assert.IsNull(customer);
    }

    [TestMethod]
    public void First_AfterWhere()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false },
            new Customer { Name = "Charlie", Age = 45, IsActive = true }
        );
        context.SaveChanges();

        var customer = context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Age)
            .First();

        Assert.AreEqual("Alice", customer.Name);
    }

    [TestMethod]
    public void Single_AfterWhere()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();

        context.Customers.AddRange(
            new Customer { Name = "Alice", Age = 25, IsActive = true },
            new Customer { Name = "Bob", Age = 35, IsActive = false }
        );
        context.SaveChanges();

        var customer = context.Customers
            .Where(c => !c.IsActive)
            .Single();

        Assert.AreEqual("Bob", customer.Name);
    }
}
