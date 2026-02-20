using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fudie.PubSub.Gcp;

public static class PubSubServiceExtensions
{
    public static IServiceCollection AddPubSubGcp(this IServiceCollection services, IConfiguration configuration)
    {
        var projectId = configuration["PubSub:ProjectId"]
            ?? throw new InvalidOperationException("PubSub:ProjectId is not configured");

        services.AddSingleton<IPubSubClient>(_ => new GcpPubSubClient(projectId));

        return services;
    }
}
