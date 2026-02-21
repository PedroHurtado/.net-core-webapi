namespace Fudie.PubSub.Gcp;

public static class PubSubServiceExtensions
{
    public static IServiceCollection AddPubSubGcp(this IServiceCollection services, IConfiguration configuration)
    {
        var projectId = configuration["PubSub:ProjectId"]
            ?? throw new InvalidOperationException("PubSub:ProjectId is not configured");

        services.AddSingleton<IPubSubClient>(sp =>
            new GcpPubSubClient(projectId, sp.GetService<ISerializer>()));

        return services;
    }
}
