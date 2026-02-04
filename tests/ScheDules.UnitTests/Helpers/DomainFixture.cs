namespace Schedules.UnitTests.Helpers;

public class DomainFixture
{
    public IServiceProvider ServiceProvider { get; }

    public DomainFixture()
    {
        var services = new ServiceCollection();
        var assembly = typeof(ServiceSchedule).Assembly;
        services.AddDomainCommands(assembly);
        ServiceProvider = services.BuildServiceProvider();
    }

    public T Get<T>() where T : class => ServiceProvider.GetRequiredService<T>();
}