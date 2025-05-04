
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskAspNet.Data.Context;
using TaskAspNet.Data.Entities;
using TaskAspNet.Data.Interfaces;

namespace TaskAspNet.Data.Repositories;
// asked chatgpt4.5 to make this code cleaner that is why the lambda expression is used.
// The lamba expression is instead of the basic return statement.
public class ConsentRepository : IConsentRepository
{
    private readonly IBaseRepository<Consent> _baseRepository;
    private readonly AppDbContext _context;

    public ConsentRepository(IBaseRepository<Consent> baseRepository, AppDbContext context)
    {
        _baseRepository = baseRepository;
        _context = context;
    }

    public async Task<Consent?> GetByUserIdAsync(string userId)
    {
        return await _context
            .Set<Consent>()
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public Task<Consent> AddAsync(Consent entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        _context.Set<Consent>().AddAsync(entity);
        return Task.FromResult(entity);
    }


    public Task<IEnumerable<Consent>> FindAsync(Expression<Func<Consent, bool>> predicate)
        => _baseRepository.FindAsync(predicate);

    public Task<IEnumerable<Consent>> GetAllAsync()
        => _baseRepository.GetAllAsync();

    public Task<Consent> GetByIdAsync(int id)
        => _baseRepository.GetByIdAsync(id);

    public Task<Consent> UpdateAsync(Consent entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        _context.Set<Consent>().Update(entity);
        return Task.FromResult(entity);
    }

    public Task<Consent> DeleteAsync(Consent entity)
        => _baseRepository.DeleteAsync(entity);

    public Task SaveAsync()
        => _baseRepository.SaveAsync();
}
