namespace Subscriptions.Infrastructure;

public class SubscriptionsDbContext(DbContextOptions<SubscriptionsDbContext> options) :
    FudieDbContext(options)
{
}
