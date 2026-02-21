namespace Fudie.PubSub.UnitTests.Hosting;

public class AddPubSubMessagingTests
{
    [Fact]
    public void AddPubSubMessaging_RegistersMessageContext_AsScoped()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PubSub:ProjectId"] = "test-project"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddPubSubMessaging(pubsub => pubsub.UseGcp(configuration));

        var descriptor = services.First(d => d.ServiceType == typeof(IMessageContext));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddPubSubMessaging_RegistersMessagePublisher_AsScoped()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PubSub:ProjectId"] = "test-project"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddPubSubMessaging(pubsub => pubsub.UseGcp(configuration));

        var descriptor = services.First(d => d.ServiceType == typeof(IMessagePublisher));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddPubSubMessaging_RegistersMessageHost_AsSingleton()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PubSub:ProjectId"] = "test-project"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddPubSubMessaging(pubsub => pubsub.UseGcp(configuration));

        var descriptor = services.First(d => d.ServiceType == typeof(MessageHost));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPubSubMessaging_RegistersIPubSubClient_AsSingleton()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PubSub:ProjectId"] = "test-project"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddPubSubMessaging(pubsub => pubsub.UseGcp(configuration));

        var descriptor = services.First(d => d.ServiceType == typeof(IPubSubClient));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void UseGcp_ThrowsWhenProjectIdMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([])
            .Build();

        var services = new ServiceCollection();

        var act = () => services.AddPubSubMessaging(pubsub => pubsub.UseGcp(configuration));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ProjectId*");
    }
}
