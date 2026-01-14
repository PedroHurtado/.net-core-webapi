using Microsoft.EntityFrameworkCore;

namespace Customer.Infrastructure;

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
