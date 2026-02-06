namespace Customers.Infrastructure;

public class CustomersDbContext(DbContextOptions<CustomersDbContext> options) :
    DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }
}