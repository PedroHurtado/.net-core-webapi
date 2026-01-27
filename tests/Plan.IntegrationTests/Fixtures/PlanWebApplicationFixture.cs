namespace Plans.IntegrationTests.Fixtures;

public class PlanWebApplicationFixture : IClassFixture<WebApplicationFactory<Program>>
{
    private const string EmulatorHost = "127.0.0.1:8080";

    private readonly WebApplicationFactory<Program> _factory;

    public HttpClient Client { get; }
    public IServiceProvider Services => _factory.Services;

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public PlanWebApplicationFixture(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", EmulatorHost);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PlanDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<PlanDbContext>(options =>
                {
                    options.UseFirestore("demo-project");
                }, ServiceLifetime.Scoped);

                services.AddScoped<IChangeTracker>(sp => sp.GetRequiredService<PlanDbContext>());
                services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PlanDbContext>());
                services.AddScoped<IQuery>(sp => sp.GetRequiredService<PlanDbContext>());
                services.AddScoped<IEntityLookup>(sp => sp.GetRequiredService<PlanDbContext>());
            });
        });

        Client = _factory.CreateClient();
    }

    public T GetService<T>() where T : notnull
    {
        using var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    public async Task<T> ExecuteWithDbContext<T>(Func<PlanDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlanDbContext>();
        return await action(dbContext);
    }
}
