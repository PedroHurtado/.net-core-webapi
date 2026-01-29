namespace Plans.Infrastructure;

public class PlanDbContext(DbContextOptions<PlanDbContext> options) :
    DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    // Root collections (aggregates)
    public DbSet<Plan> Plans => Set<Plan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);

        modelBuilder.Entity<Plan>(entity =>
        {
            // Ignore: propiedades computed (no backing fields)
            entity.Ignore(p => p.HasActiveProvider);            

            // ComplexType: Price (Money con Currency anidado)
            entity.ComplexProperty(p => p.Price, price =>
            {
                // Ignore: propiedades computed de Money
                price.Ignore(m => m.IsZero);
                price.Ignore(m => m.IsPositive);
                price.Ignore(m => m.IsNegative);

                price.ComplexProperty(m => m.Currency);
            });

            // ArrayOf: Features (usa backing field _features)
            entity.ArrayOf(p => p.Features, feature =>
            {
                // Ignore: propiedades computed de Feature
                feature.Ignore(f => f.IsValid);
                feature.Ignore(f => f.DisplayValue);
            });

            // ArrayOf: ProviderConfigurations (usa backing field _providerConfigurations)
            entity.ArrayOf(p => p.ProviderConfigurations);
        });
    }

    public async Task<T> GetRequiredAsync<T, TId>(
        TId id,
        bool tracking = true,
        CancellationToken cancellationToken = default,
        params string[] includeProperties) where T : class, IEntity<TId> where TId : notnull
    {
        var entity = await Set<T>().FindAsync([id], cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"{typeof(T).Name} with ID '{id}' not found.");
        }

        foreach (var includeProperty in includeProperties)
        {
            await Entry(entity).Navigation(includeProperty).LoadAsync(cancellationToken);
        }

        if (!tracking)
        {
            Entry(entity).State = EntityState.Detached;
        }

        return entity;
    }

    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }
}
