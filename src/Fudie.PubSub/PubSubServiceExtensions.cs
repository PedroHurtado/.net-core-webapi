using Fudie.PubSub.Gcp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fudie.PubSub;

public static class PubSubServiceExtensions
{
    public static IServiceCollection AddPubSub(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["PubSub:Provider"]
            ?? throw new InvalidOperationException("PubSub:Provider is not configured");

        var projectId = configuration["PubSub:ProjectId"]
            ?? throw new InvalidOperationException("PubSub:ProjectId is not configured");

        if (provider is not "Gcp")
            throw new InvalidOperationException($"Unknown PubSub provider: '{provider}'");

        services.AddSingleton<IPubSubClient>(_ => provider switch
        {
            "Gcp" => new GcpPubSubClient(projectId),
            _ => throw new InvalidOperationException($"Unknown PubSub provider: '{provider}'")
        });

        return services;
    }
}
