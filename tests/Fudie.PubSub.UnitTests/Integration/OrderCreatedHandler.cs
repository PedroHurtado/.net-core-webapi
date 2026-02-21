namespace Fudie.PubSub.UnitTests.Integration;

public record OrderCreated(string Description);

public class OrderCreatedHandler(TestOrdersDbContext db, IMessageContext context) : IMessageHandler<OrderCreated>
{
    public async Task Handle(OrderCreated message, CancellationToken ct)
    {
        db.Orders.Add(new TestOrder
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Parse(context.TenantId!),
            Description = message.Description
        });

        await db.SaveChangesAsync(ct);
    }
}
