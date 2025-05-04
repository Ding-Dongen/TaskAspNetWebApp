
using Microsoft.EntityFrameworkCore;
using TaskAspNet.Business.Interfaces;


namespace TaskAspNet.Tests;

public class FakeUnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly DbContext _context;

    public FakeUnitOfWork(DbContext context) => _context = context;

    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;

    public Task<int> SaveAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public Task CommitAsync(CancellationToken ct = default)
        => SaveAsync(ct);

    public Task RollbackAsync(CancellationToken ct = default)
        => Task.CompletedTask;

    public ValueTask DisposeAsync()
        => ValueTask.CompletedTask;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        var result = await action();
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task ExecuteAsync(Func<Task> action)
    {
        await action();
        await _context.SaveChangesAsync();
    }
}
