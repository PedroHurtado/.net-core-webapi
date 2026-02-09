namespace Auth.UnitTests.Helpers;

public class DomainFixture
{
    public IServiceProvider ServiceProvider { get; }

    public DomainFixture()
    {
        var services = new ServiceCollection();
        var assembly = typeof(AuthDbContext).Assembly;
        services.AddDomainCommands(assembly);
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        ServiceProvider = services.BuildServiceProvider();
    }

    public T Get<T>() where T : class => ServiceProvider.GetRequiredService<T>();
}
