using Microsoft.EntityFrameworkCore;
using Fudie.Domain;
using Fudie.Infrastructure;
using webapi.features.ingredients.models;
using webapi.features.pizzas.models;

namespace webapi.infrastructure;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
        DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{


    public required DbSet<Ingredient> Ingredients { get; set; }
    public required DbSet<Pizza> Pizzas { get; set; }

    public async Task<T> GetRequiredAsync<T, ID>(
    ID id,
    bool tracking = true,
    CancellationToken cancellationToken = default,
    params string[] includeProperties) where T : Entity
    {
        var query = Set<T>().AsQueryable();

        // Aplicar includes
        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var entity = await query.Where(e => e.Id.Equals(id)).FirstOrDefaultAsync(cancellationToken);
        return entity ?? throw new KeyNotFoundException($"{typeof(T).Name} with ID '{id}' not found.");
    }

    public IQueryable<T> Query<T>() where T : Entity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }

    /*public async Task<List<T>> ToListAsync<T>(IQueryable<T> query)
    {
       return await query.ToListAsync();
    }*/

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}