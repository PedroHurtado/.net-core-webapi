namespace Auth.Infrastructure;

public class AuthDbContext(DbContextOptions<AuthDbContext> options, Guid tenantId) :
    FudieDbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<TenantRole> TenantRoles => Set<TenantRole>();

    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<ExternalApp> ExternalApps => Set<ExternalApp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
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


        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantId);

            entity.Reference(x=>x.Role);
            entity.Reference(x=>x.User);

        });

        modelBuilder.Entity<ExternalApp>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantId);
            entity.Reference(x => x.User);
        });
    }
}
