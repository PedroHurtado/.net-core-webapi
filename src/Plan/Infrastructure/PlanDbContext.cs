using Microsoft.EntityFrameworkCore;

namespace Plan.Infrastructure;

public class PlanDbContext(DbContextOptions<PlanDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Global: use backing fields for all properties
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
