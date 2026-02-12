namespace Subscriptions.Infrastructure;

public class SubscriptionsDbContext(DbContextOptions<SubscriptionsDbContext> options) :
    DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
