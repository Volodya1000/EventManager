namespace EventManager.Domain.Interfaces;

public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cst = default);
    Task CommitAsync(CancellationToken cst = default);
    Task RollbackAsync(CancellationToken cst = default);
}