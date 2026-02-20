namespace Fudie.PubSub.UnitTests;

public class PubSubServiceExtensionsTests
{
    [Fact]
    public void AddPubSubGcp_RegistersIPubSubClient_AsSingleton()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PubSub:ProjectId"] = "test-project"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddPubSubGcp(configuration);

        var descriptor = services.First(d => d.ServiceType == typeof(IPubSubClient));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPubSubGcp_ThrowsWhenProjectIdMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([])
            .Build();

        var services = new ServiceCollection();

        var act = () => services.AddPubSubGcp(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ProjectId*");
    }
}
