using EventManager.Domain.Interfaces;
using EventManager.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync(CancellationToken cst = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cst);
    }

    public async Task CommitAsync(CancellationToken cst = default)
    {
        try
        {
            await _context.SaveChangesAsync(cst);
            await _transaction.CommitAsync(cst);
        }
        catch
        {
            await RollbackAsync(cst);
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken cst = default)
    {
        await _transaction.RollbackAsync(cst);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}