namespace Auth.IntegrationTests.Fixtures;

public class AuthWebApplicationFixture : IClassFixture<WebApplicationFactory<Program>>
{
    private const string EmulatorHost = "127.0.0.1:8080";

    protected readonly WebApplicationFactory<Program> _factory;

    public HttpClient Client { get; }

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AuthWebApplicationFixture(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", EmulatorHost);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var tenantId = Guid.NewGuid();

                services.AddDbContext<AuthDbContext>(options =>
                {
                    options.UseFirestore("demo-project");
                }, ServiceLifetime.Scoped);

                services.AddScoped(typeof(Guid), _ => tenantId);

                services.AddScoped<IChangeTracker>(sp => sp.GetRequiredService<AuthDbContext>());
                services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AuthDbContext>());
                services.AddScoped<IQuery>(sp => sp.GetRequiredService<AuthDbContext>());
                services.AddScoped<IEntityLookup>(sp => sp.GetRequiredService<AuthDbContext>());
            });
        });

        Client = _factory.CreateClient();
    }
}
