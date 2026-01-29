namespace Customer.Infrastructure;

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options, Guid tenantId) :
    DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    // Root collections (aggregates)
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Allergen> Allergens => Set<Allergen>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Global: use backing fields for all properties
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);

        // Menu aggregate configuration
        modelBuilder.Entity<Menu>(entity =>
        {
            // QueryFilter: multi-tenancy by TenantId
            entity.HasQueryFilter(m => m.TenantId == tenantId);

            // ComplexType: DepositPolicy (embedded object, no Id)
            entity.ComplexProperty(m => m.DepositPolicy);

            // SubCollection: Categories under Menu
            entity.SubCollection(m => m.Categories, category =>
            {
                // ArrayOf embedded: CategoryItem contains Reference to MenuItem
                category.ArrayOf(c => c.Items, item =>
                {
                    item.Reference(i => i.MenuItem);
                });
            });
        });

        // MenuItem aggregate configuration
        modelBuilder.Entity<MenuItem>(entity =>
        {
            // QueryFilter: multi-tenancy by TenantId
            entity.HasQueryFilter(m => m.TenantId == tenantId);

            // Ignore computed properties (no backing field)
            entity.Ignore(m => m.IsAvailableToday);
            entity.Ignore(m => m.CanBeOrdered);
            entity.Ignore(m => m.HasDepositOverride);
            entity.Ignore(m => m.HasActivePriceOption);

            // ComplexTypes (embedded objects)
            entity.ComplexProperty(m => m.DepositOverride, complex =>
            {
                complex.Ignore(d => d!.AppliesToAllQuantities);
            });
            entity.ComplexProperty(m => m.NutritionalInfo);

            // ArrayOf embedded: PriceOptions
            entity.ArrayOf(m => m.PriceOptions, priceOption =>
            {
                priceOption.Ignore(p=>p.DisplayPrice);
                priceOption.Ignore(p=>p.RequiresMarketPrice);
            });

            // ArrayOf Reference: Allergens (references to Allergen aggregate)
            entity.ArrayOf(m => m.Allergens).AsReferences();
        });

        // Allergen is simple - conventions handle it automatically
        // (Id as PK, collection name pluralized, etc.)
    }

    

    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }
}
