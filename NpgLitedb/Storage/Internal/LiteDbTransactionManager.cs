using Microsoft.EntityFrameworkCore.Storage;

namespace NpgLitedb.Storage.Internal;

/// <summary>
/// Transaction manager for LiteDB. LiteDB handles transactions internally,
/// so this is a lightweight implementation.
/// </summary>
public class LiteDbTransactionManager : IDbContextTransactionManager
{
    /// <summary>
    /// Creates a new instance of <see cref="LiteDbTransactionManager"/>.
    /// </summary>
    public LiteDbTransactionManager()
    {
    }

    /// <inheritdoc />
    public IDbContextTransaction CurrentTransaction => null!;

    /// <inheritdoc />
    public IDbContextTransaction BeginTransaction()
        => new LiteDbTransaction();

    /// <inheritdoc />
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IDbContextTransaction>(new LiteDbTransaction());

    /// <inheritdoc />
    public void CommitTransaction() { }

    /// <inheritdoc />
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public void RollbackTransaction() { }

    /// <inheritdoc />
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public void ResetState() { }

    /// <inheritdoc />
    public Task ResetStateAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    private sealed class LiteDbTransaction : IDbContextTransaction
    {
        public Guid TransactionId { get; } = Guid.NewGuid();

        public void Commit() { }

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Rollback() { }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Dispose() { }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
