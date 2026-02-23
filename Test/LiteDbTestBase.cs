using Test.DAL;

namespace Test;

/// <summary>
/// Base class for LiteDB provider tests. Provides a unique temp DB path
/// per test and cleans up the files after each test.
/// </summary>
public abstract class LiteDbTestBase
{
    protected string DbPath { get; private set; } = null!;

    protected TestDbContext CreateContext() => new(DbPath);

    [TestInitialize]
    public void BaseSetup()
    {
        DbPath = Path.Combine(Path.GetTempPath(), $"litedb_test_{Guid.NewGuid():N}.db");
    }

    [TestCleanup]
    public void BaseCleanup()
    {
        try { File.Delete(DbPath); } catch { }
        try { File.Delete(DbPath + "-log"); } catch { }
    }
}
