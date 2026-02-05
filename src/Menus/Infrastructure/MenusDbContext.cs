namespace Menus.Infrastructure;

public class MenusDbContext : DbContext, IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    private readonly Guid _tenantId;

    public MenusDbContext(DbContextOptions<MenusDbContext> options, Guid tenantId)
        : base(options)
    {
        _tenantId = tenantId;

       
    }

    // Root collections (aggregates)
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Allergen> Allergens => Set<Allergen>();    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasQueryFilter(m => m.TenantId == _tenantId);

            entity.ComplexProperty(m => m.DepositPolicy);

            entity.SubCollection(m => m.Categories, category =>
            {
                /*category.Entity(builder =>
                {
                    builder.Property(c => c.Id).ValueGeneratedNever();                   
                });*/

                category.ArrayOf(c => c.Items, item =>
                {
                    item.Reference(i => i.MenuItem);
                });
            });
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasQueryFilter(m => m.TenantId == _tenantId);

            entity.Ignore(m => m.IsAvailableToday);
            entity.Ignore(m => m.CanBeOrdered);
            entity.Ignore(m => m.HasDepositOverride);
            entity.Ignore(m => m.HasActivePriceOption);

            entity.ComplexProperty(m => m.DepositOverride, complex =>
            {
                complex.Ignore(d => d!.AppliesToAllQuantities);
            });
            entity.ComplexProperty(m => m.NutritionalInfo);

            entity.ArrayOf(m => m.PriceOptions, priceOption =>
            {
                priceOption.Ignore(p => p.DisplayPrice);
                priceOption.Ignore(p => p.RequiresMarketPrice);
            });

            entity.ArrayOf(m => m.Allergens).AsReferences();
        });
    }

    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }
}