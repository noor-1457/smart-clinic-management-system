using System.Linq.Expressions;

namespace smart_clinic_management.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> Query();
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    Task SaveChangesAsync();
}

