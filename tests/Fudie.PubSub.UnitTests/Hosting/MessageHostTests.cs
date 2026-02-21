namespace Fudie.PubSub.UnitTests.Hosting;

public record TestEvent(string Description);

public class TestEventHandler : IMessageHandler<TestEvent>
{
    public TestEvent? Received { get; private set; }
    public IMessageContext? CapturedContext { get; private set; }

    public Task Handle(TestEvent message, CancellationToken ct)
    {
        Received = message;
        return Task.CompletedTask;
    }
}

public class ContextCapturingHandler(IMessageContext context) : IMessageHandler<TestEvent>
{
    public IMessageContext? CapturedContext { get; private set; }
    public TestEvent? Received { get; private set; }

    public Task Handle(TestEvent message, CancellationToken ct)
    {
        CapturedContext = context;
        Received = message;
        return Task.CompletedTask;
    }
}

public class MessageHostTests
{
    [Fact]
    public async Task SubscribeAsync_DispatchesToHandler()
    {
        var envelope = new Envelope<TestEvent>(
            MessageId: Guid.NewGuid().ToString(),
            CorrelationId: null,
            Type: nameof(TestEvent),
            OccurredAt: DateTime.UtcNow,
            Claims: null,
            Payload: new TestEvent("test message")
        );

        var serializer = new JsonPubSubSerializer();
        var stub = new StubPubSubClient { DataToDeliver = serializer.Serialize(envelope) };
        var handler = new TestEventHandler();

        var services = new ServiceCollection();
        services.AddScoped<MessageContext>();
        services.AddScoped<IMessageContext>(sp => sp.GetRequiredService<MessageContext>());
        services.AddScoped<IMessageHandler<TestEvent>>(_ => handler);
        var provider = services.BuildServiceProvider();

        var host = new MessageHost(stub, provider);

        await host.SubscribeAsync<TestEvent>("test-sub");

        handler.Received.Should().Be(new TestEvent("test message"));
    }

    [Fact]
    public async Task SubscribeAsync_PopulatesMessageContext()
    {
        var envelope = new Envelope<TestEvent>(
            MessageId: Guid.NewGuid().ToString(),
            CorrelationId: "corr-99",
            Type: nameof(TestEvent),
            OccurredAt: DateTime.UtcNow,
            Claims: new Dictionary<string, string>
            {
                ["tenant_id"] = "tenant-42",
                ["sub"] = "user-7"
            },
            Payload: new TestEvent("with context")
        );

        var serializer = new JsonPubSubSerializer();
        var stub = new StubPubSubClient { DataToDeliver = serializer.Serialize(envelope) };

        ContextCapturingHandler? capturedHandler = null;

        var services = new ServiceCollection();
        services.AddScoped<MessageContext>();
        services.AddScoped<IMessageContext>(sp => sp.GetRequiredService<MessageContext>());
        services.AddScoped<IMessageHandler<TestEvent>>(sp =>
        {
            capturedHandler = new ContextCapturingHandler(sp.GetRequiredService<IMessageContext>());
            return capturedHandler;
        });
        var provider = services.BuildServiceProvider();

        var host = new MessageHost(stub, provider);

        await host.SubscribeAsync<TestEvent>("test-sub");

        capturedHandler.Should().NotBeNull();
        capturedHandler!.CapturedContext!.TenantId.Should().Be("tenant-42");
        capturedHandler.CapturedContext.UserId.Should().Be("user-7");
        capturedHandler.CapturedContext.CorrelationId.Should().Be("corr-99");
    }

    [Fact]
    public async Task SubscribeAsync_WithNoClaims_ContextRemainsEmpty()
    {
        var envelope = new Envelope<TestEvent>(
            MessageId: Guid.NewGuid().ToString(),
            CorrelationId: null,
            Type: nameof(TestEvent),
            OccurredAt: DateTime.UtcNow,
            Claims: null,
            Payload: new TestEvent("no claims")
        );

        var serializer = new JsonPubSubSerializer();
        var stub = new StubPubSubClient { DataToDeliver = serializer.Serialize(envelope) };

        ContextCapturingHandler? capturedHandler = null;

        var services = new ServiceCollection();
        services.AddScoped<MessageContext>();
        services.AddScoped<IMessageContext>(sp => sp.GetRequiredService<MessageContext>());
        services.AddScoped<IMessageHandler<TestEvent>>(sp =>
        {
            capturedHandler = new ContextCapturingHandler(sp.GetRequiredService<IMessageContext>());
            return capturedHandler;
        });
        var provider = services.BuildServiceProvider();

        var host = new MessageHost(stub, provider);

        await host.SubscribeAsync<TestEvent>("test-sub");

        capturedHandler!.CapturedContext!.TenantId.Should().BeNull();
        capturedHandler.CapturedContext.UserId.Should().BeNull();
        capturedHandler.CapturedContext.Claims.Should().BeEmpty();
    }
}
