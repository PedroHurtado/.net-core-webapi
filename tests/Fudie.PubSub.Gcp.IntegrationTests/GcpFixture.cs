namespace Fudie.PubSub.Gcp.IntegrationTests;

public class GcpFixture
{
    public IPubSubClient Client { get; }

    public GcpFixture()
    {
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "localhost:8085");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PubSub:ProjectId"] = "demo-project"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddPubSubGcp(configuration);

        var provider = services.BuildServiceProvider();
        Client = provider.GetRequiredService<IPubSubClient>();
    }
}
