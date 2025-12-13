using Microsoft.EntityFrameworkCore;
using smart_clinic_management.Data;

namespace smart_clinic_management.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ClinicDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ClinicDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public Task<T?> GetByIdAsync(Guid id) => _dbSet.FindAsync(id).AsTask();

    public Task<T?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate) =>
        _dbSet.FirstOrDefaultAsync(predicate);

    public IQueryable<T> Query() => _dbSet.AsQueryable();

    public Task AddAsync(T entity) => _dbSet.AddAsync(entity).AsTask();

    public Task AddRangeAsync(IEnumerable<T> entities) => _dbSet.AddRangeAsync(entities);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}

