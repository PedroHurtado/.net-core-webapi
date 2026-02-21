namespace Fudie.PubSub.Hosting;

public static class MessagingServiceExtensions
{
    public static IServiceCollection AddPubSubMessaging(
        this IServiceCollection services,
        Action<PubSubBuilder> configure)
    {
        var builder = new PubSubBuilder(services);
        configure(builder);

        services.AddScoped<MessageContext>();
        services.AddScoped<IMessageContext>(sp => sp.GetRequiredService<MessageContext>());
        services.AddScoped<IMessagePublisher, MessagePublisher>();
        services.AddSingleton<MessageHost>();

        return services;
    }
}
