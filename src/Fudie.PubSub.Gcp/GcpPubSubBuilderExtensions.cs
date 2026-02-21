namespace Fudie.PubSub.Gcp;

public static class GcpPubSubBuilderExtensions
{
    public static PubSubBuilder UseGcp(this PubSubBuilder builder, IConfiguration configuration)
    {
        var projectId = configuration["PubSub:ProjectId"]
            ?? throw new InvalidOperationException("PubSub:ProjectId is not configured");

        builder.Services.AddSingleton<IPubSubClient>(sp =>
            new GcpPubSubClient(projectId, sp.GetService<ISerializer>()));

        return builder;
    }
}
