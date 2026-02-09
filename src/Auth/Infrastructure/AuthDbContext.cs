namespace Auth.Infrastructure;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) :
    DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Ignore(u => u.IsOAuth);
            entity.Ignore(u => u.HasPassword);

            entity.ComplexProperty(u => u.Password);
        });
    }
}
