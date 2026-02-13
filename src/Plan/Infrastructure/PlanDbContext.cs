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
            entity.Ignore(p => p.HasActivePricingTierWithProvider);

            // ArrayOf: Features (usa backing field _features)
            entity.ArrayOf(p => p.Features, feature =>
            {
                // Ignore: propiedades computed de Feature
                feature.Ignore(f => f.IsValid);
                feature.Ignore(f => f.DisplayValue);                
            });

            // ArrayOf: PricingTiers (usa backing field _pricingTiers)
            entity.ArrayOf(p => p.PricingTiers, tier =>
            {
                // Ignore: propiedades computed de PricingTier
                tier.Ignore(t => t.HasActiveProvider);
            });
        });
    }

    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }
}
