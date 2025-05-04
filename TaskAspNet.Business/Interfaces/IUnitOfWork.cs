using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAspNet.Business.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task BeginAsync(CancellationToken ct = default);
        Task<int> SaveAsync(CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
    }
}
