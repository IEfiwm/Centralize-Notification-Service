using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NotificationCenter.Application.Abstractions.Persistence;

namespace NotificationCenter.Infrastructure.Persistence;

public class BaseRepository<T>(AppDbContext db) : IRepository<T> where T : class
{
    public IQueryable<T> Query() => db.Set<T>().AsQueryable();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Set<T>().FindAsync([id], ct).AsTask();

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct) =>
        db.Set<T>().FirstOrDefaultAsync(predicate, ct);

    public Task AddAsync(T entity, CancellationToken ct) =>
        db.Set<T>().AddAsync(entity, ct).AsTask();

    Task IRepository<T>.UpdateAsync(T entity, CancellationToken ct)
    {
        db.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

