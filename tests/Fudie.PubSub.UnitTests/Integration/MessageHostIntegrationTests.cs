namespace Fudie.PubSub.UnitTests.Integration;

public class MessageHostIntegrationTests
{
    private const string EmulatorHost = "127.0.0.1:8080";
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task FullPipeline_MessageArrives_HandlerCreatesEntityWithCorrectTenant()
    {
        Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", EmulatorHost);

        var envelope = new Envelope<OrderCreated>(
            MessageId: Guid.NewGuid().ToString(),
            CorrelationId: "corr-integration",
            Type: nameof(OrderCreated),
            OccurredAt: DateTime.UtcNow,
            Claims: new Dictionary<string, string>
            {
                ["tenant_id"] = _tenantId.ToString(),
                ["sub"] = "user-integration"
            },
            Payload: new OrderCreated("Pizza Margherita")
        );

        var serializer = new JsonPubSubSerializer();
        var stub = new StubPubSubClient { DataToDeliver = serializer.Serialize(envelope) };

        var services = new ServiceCollection();
        services.AddScoped<MessageContext>();
        services.AddScoped<IMessageContext>(sp => sp.GetRequiredService<MessageContext>());
        services.AddDbContext<TestOrdersDbContext>(opts => opts.UseFirestore("demo-project"));
        services.AddScoped<IMessageHandler<OrderCreated>, OrderCreatedHandler>();
        var provider = services.BuildServiceProvider();

        var host = new MessageHost(stub, provider);

        await host.SubscribeAsync<OrderCreated>("test-sub");

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MessageContext>();
        context.Populate(
            new Dictionary<string, string> { ["tenant_id"] = _tenantId.ToString() },
            null
        );
        var db = scope.ServiceProvider.GetRequiredService<TestOrdersDbContext>();
        var orders = await db.Orders.ToListAsync();

        orders.Should().ContainSingle();
        orders[0].Description.Should().Be("Pizza Margherita");
        orders[0].TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task FullPipeline_QueryFilter_OnlyReturnsTenantOrders()
    {
        Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", EmulatorHost);

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var services = new ServiceCollection();
        services.AddScoped<MessageContext>();
        services.AddScoped<IMessageContext>(sp => sp.GetRequiredService<MessageContext>());
        services.AddDbContext<TestOrdersDbContext>(opts => opts.UseFirestore("demo-project"));
        services.AddScoped<IMessageHandler<OrderCreated>, OrderCreatedHandler>();
        var provider = services.BuildServiceProvider();

        var serializer = new JsonPubSubSerializer();

        var envelopeA = new Envelope<OrderCreated>(
            MessageId: Guid.NewGuid().ToString(),
            CorrelationId: null,
            Type: nameof(OrderCreated),
            OccurredAt: DateTime.UtcNow,
            Claims: new Dictionary<string, string> { ["tenant_id"] = tenantA.ToString() },
            Payload: new OrderCreated("Order from Tenant A")
        );

        var envelopeB = new Envelope<OrderCreated>(
            MessageId: Guid.NewGuid().ToString(),
            CorrelationId: null,
            Type: nameof(OrderCreated),
            OccurredAt: DateTime.UtcNow,
            Claims: new Dictionary<string, string> { ["tenant_id"] = tenantB.ToString() },
            Payload: new OrderCreated("Order from Tenant B")
        );

        var stubA = new StubPubSubClient { DataToDeliver = serializer.Serialize(envelopeA) };
        var hostA = new MessageHost(stubA, provider);
        await hostA.SubscribeAsync<OrderCreated>("sub-a");

        var stubB = new StubPubSubClient { DataToDeliver = serializer.Serialize(envelopeB) };
        var hostB = new MessageHost(stubB, provider);
        await hostB.SubscribeAsync<OrderCreated>("sub-b");

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MessageContext>();
        context.Populate(
            new Dictionary<string, string> { ["tenant_id"] = tenantA.ToString() },
            null
        );
        var db = scope.ServiceProvider.GetRequiredService<TestOrdersDbContext>();
        var orders = await db.Orders.ToListAsync();

        orders.Should().ContainSingle();
        orders[0].Description.Should().Be("Order from Tenant A");
    }
}
