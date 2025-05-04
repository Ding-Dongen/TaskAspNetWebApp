
using Microsoft.EntityFrameworkCore.Storage;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Data.Context;

namespace TaskAspNet.Business.Services;

public sealed class UnitOfWork(AppDbContext ctx) : IUnitOfWork
{
    private readonly AppDbContext _ctx = ctx;
    private IDbContextTransaction? _tx;

    public async Task BeginAsync(CancellationToken ct = default)
    {
        if (_tx is null)
            _tx = await _ctx.Database.BeginTransactionAsync(ct);
    }

    public Task<int> SaveAsync(CancellationToken ct = default) =>
        _ctx.SaveChangesAsync(ct);

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await SaveAsync(ct);
        if (_tx is not null) await _tx.CommitAsync(ct);
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_tx is not null) await _tx.RollbackAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (_tx is not null) await _tx.DisposeAsync();
    }
}
