using Customer.Features.Menus.Domain.AllergenAggregate;
using Customer.Features.Menus.Domain.MenuAggregate;
using Customer.Features.Menus.Domain.MenuAggregate.Entities;
using Customer.Features.Menus.Domain.MenuItemAggregate;
using Fudie.Firestore.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Customer.Infrastructure;

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    // Root collections (aggregates)
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Allergen> Allergens => Set<Allergen>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Menu aggregate configuration
        modelBuilder.Entity<Menu>(entity =>
        {
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
            // ComplexTypes (embedded objects)
            entity.ComplexProperty(m => m.DepositOverride);
            entity.ComplexProperty(m => m.NutritionalInfo);

            // ArrayOf embedded: PriceOptions (no Id)
            entity.ArrayOf(m => m.PriceOptions);

            // AvailableDays: HashSet<DayOfWeek> is handled by ListEnumToStringArrayConvention

            // ArrayOf Reference: Allergens (references to Allergen aggregate)
            entity.ArrayOf(m => m.Allergens).AsReferences();
        });

        // Allergen is simple - conventions handle it automatically
        // (Id as PK, collection name pluralized, etc.)
    }
}
