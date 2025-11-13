using Microsoft.EntityFrameworkCore;
using webapi.common.dependencyinjection;
using webapi.common.domain;
using webapi.common.infrastructure;
using webapi.features.ingredients.models;

namespace webapi.infrastructure;

[Injectable]
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IGetOrThrowAsync, IQuery
{
    public DbSet<Ingredient> Ingredients { get; set; }

    public async Task<T> GetOrThrowAsync<T, ID>(ID id, bool tracking = true, CancellationToken cancellationToken = default) where T : Entity
    {
        var query = Set<T>().AsQueryable();

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

    /*public IQueryable<T> Query<T>(): where T Entity
    {
        
        var query = Set<T>().AsQueryable();
        query.AsNoTracking();
        return query;

    }*/

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}