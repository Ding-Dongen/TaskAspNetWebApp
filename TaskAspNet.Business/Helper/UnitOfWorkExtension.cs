using TaskAspNet.Business.Interfaces;

namespace TaskAspNet.Business.Helper;

public static class UnitOfWorkExtensions
{
    public static async Task ExecuteAsync(this IUnitOfWork uow, Func<Task> work, CancellationToken ct = default)
    {
        await uow.BeginAsync(ct);
        try
        {
            await work();
            await uow.CommitAsync(ct);
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }
    }

    public static async Task<T> ExecuteAsync<T>(this IUnitOfWork uow, Func<Task<T>> work, CancellationToken ct = default)
    {
        await uow.BeginAsync(ct);
        try
        {
            T result = await work();
            await uow.CommitAsync(ct);
            return result;
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }
    }
}
