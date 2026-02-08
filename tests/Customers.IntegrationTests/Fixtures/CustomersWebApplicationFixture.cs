namespace Customers.IntegrationTests.Fixtures;

public class CustomersWebApplicationFixture : IClassFixture<WebApplicationFactory<Program>>
{
    private const string EmulatorHost = "127.0.0.1:8080";

    private readonly WebApplicationFactory<Program> _factory;

    public HttpClient Client { get; }

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CustomersWebApplicationFixture(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", EmulatorHost);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CustomersDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var tenantId = Guid.NewGuid();

                services.AddDbContext<CustomersDbContext>(options =>
                {
                    options.UseFirestore("demo-project");
                }, ServiceLifetime.Scoped);

                services.AddScoped(typeof(Guid), _ => tenantId);

                services.AddScoped<IChangeTracker>(sp => sp.GetRequiredService<CustomersDbContext>());
                services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CustomersDbContext>());
                services.AddScoped<IQuery>(sp => sp.GetRequiredService<CustomersDbContext>());
                services.AddScoped<IEntityLookup>(sp => sp.GetRequiredService<CustomersDbContext>());
            });
        });

        Client = _factory.CreateClient();
    }
}
