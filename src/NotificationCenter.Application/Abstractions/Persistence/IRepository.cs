using System.Linq.Expressions;

namespace NotificationCenter.Application.Abstractions.Persistence;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
    Task AddAsync(T entity, CancellationToken ct);
    Task UpdateAsync(T entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    IQueryable<T> Query();
}

