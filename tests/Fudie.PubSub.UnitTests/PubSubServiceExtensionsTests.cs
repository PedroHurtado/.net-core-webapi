namespace Fudie.PubSub.UnitTests;

public class PubSubServiceExtensionsTests
{
    [Fact]
    public void AddPubSub_RegistersIPubSubClient_AsSingleton()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PubSub:Provider"] = "Gcp",
                ["PubSub:ProjectId"] = "test-project"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddPubSub(configuration);

        var descriptor = services.First(d => d.ServiceType == typeof(IPubSubClient));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPubSub_ThrowsWhenProviderMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PubSub:ProjectId"] = "test-project"
            })
            .Build();

        var services = new ServiceCollection();

        var act = () => services.AddPubSub(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Provider*");
    }

    [Fact]
    public void AddPubSub_ThrowsWhenProjectIdMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PubSub:Provider"] = "Gcp"
            })
            .Build();

        var services = new ServiceCollection();

        var act = () => services.AddPubSub(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ProjectId*");
    }

    [Fact]
    public void AddPubSub_ThrowsWhenProviderUnknown()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PubSub:Provider"] = "Unknown",
                ["PubSub:ProjectId"] = "test-project"
            })
            .Build();

        var services = new ServiceCollection();

        var act = () => services.AddPubSub(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unknown*");
    }
}
