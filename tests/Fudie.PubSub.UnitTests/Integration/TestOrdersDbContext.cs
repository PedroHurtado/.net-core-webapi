namespace Fudie.PubSub.UnitTests.Integration;

public class TestOrdersDbContext(DbContextOptions<TestOrdersDbContext> options, IMessageContext messageContext)
    : DbContext(options)
{
    private readonly IMessageContext _messageContext = messageContext;

    public DbSet<TestOrder> Orders => Set<TestOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestOrder>(entity =>
        {
            entity.HasQueryFilter(o => o.TenantId == Guid.Parse(_messageContext.TenantId!));
        });
    }
}
