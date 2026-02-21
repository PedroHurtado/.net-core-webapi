namespace Fudie.PubSub.Hosting;

public class PubSubBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}
