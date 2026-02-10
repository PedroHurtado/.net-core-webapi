namespace Auth.Infrastructure;

public class AuthDbContext(DbContextOptions<AuthDbContext> options, Guid tenantId) :
    DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<TenantRole> TenantRoles => Set<TenantRole>();

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

        modelBuilder.Entity<Session>(entity =>
        {
            entity.Ignore(s => s.IsExpired);
            entity.Ignore(s => s.HasTenantContext);
        });

        modelBuilder.Entity<TenantRole>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantId);
        });
    }
}
