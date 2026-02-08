namespace Customers.Infrastructure;

public class CustomersDbContext(DbContextOptions<CustomersDbContext> options) :
    DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();

    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Ignore(r => r.HasPriceRange);
            entity.Ignore(r => r.HasLogo);
            entity.Ignore(r => r.HasImages);
            entity.Ignore(r => r.CoverImage);
            entity.Ignore(r => r.IsProfileComplete);

            entity.ComplexProperty(r => r.Address, address =>
            {
                address.Ignore(a => a.FullAddress);
                address.ComplexProperty(a => a.Location);
            });

            entity.ComplexProperty(r => r.ContactInfo);

            entity.ComplexProperty(r => r.BillingInfo, billing =>
            {
                billing.ComplexProperty(b => b.BillingAddress, billingAddress =>
                {
                    billingAddress.Ignore(a => a.FullAddress);
                    billingAddress.ComplexProperty(a => a.Location);
                });
            });

            entity.ComplexProperty(r => r.PriceRange);

            entity.ArrayOf(r => r.Images, image => { });

            entity.ArrayOf(r => r.SupportedCultures);

            entity.ArrayOf(r => r.SocialLinks);

            entity.ArrayOf(r => r.CuisineTypes);
            entity.ArrayOf(r => r.ServiceAmenities);
            entity.ArrayOf(r => r.DietaryOptions);
        });
    }
}