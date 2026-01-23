namespace Plan.Infrastructure;

public class PlanDbContext(DbContextOptions<PlanDbContext> options) :
    DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    // Root collections (aggregates)
    public DbSet<PlanAgg> Plans => Set<PlanAgg>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Global: use backing fields for all properties
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);

        // Plan aggregate configuration
        modelBuilder.Entity<PlanAgg>(entity =>
        {
            // ComplexType: Price (Money value object, no Id)
            entity.ComplexProperty(p => p.Price);            

            // ArrayOf embedded: Features
            entity.ArrayOf(p => p.Features);

            // ArrayOf embedded: ProviderConfigurations
            entity.ArrayOf(p => p.ProviderConfigurations);

            entity.Ignore(p=>p.HasActiveProvider);
        });
    }

    public async Task<T> GetRequiredAsync<T, ID>(
        ID id,
        bool tracking = true,
        CancellationToken cancellationToken = default,
        params string[] includeProperties) where T : class, IEntity
    {
        var query = Set<T>().AsQueryable();

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var entity = await query.FirstOrDefaultAsync(cancellationToken);
        return entity ?? throw new KeyNotFoundException($"{typeof(T).Name} with ID '{id}' not found.");
    }

    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }
}
