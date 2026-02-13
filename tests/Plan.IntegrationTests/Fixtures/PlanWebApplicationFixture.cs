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

    public async Task<PlanResponse> CreatePlanAsync(
        string name = "Plan Test",
        string description = "Descripción de test")
    {
        var request = new CreatePlan.Request(name, description);
        var response = await Client.PostAsJsonAsync("/plans", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions))!;
    }

    public async Task<PlanResponse> AddFeatureToPlanAsync(
        Guid planId,
        string code = "FEATURE_01",
        string name = "Feature One",
        string? description = "Description",
        string type = "Boolean",
        int? limit = null,
        string? unit = null)
    {
        var request = new { Code = code, Name = name, Description = description, Type = type, Limit = limit, Unit = unit };
        var response = await Client.PostAsJsonAsync($"/plans/{planId}/features", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions))!;
    }

    public async Task<PlanResponse> AddPricingTierToPlanAsync(
        Guid planId,
        string billingPeriod = "Monthly",
        decimal amount = 9.99m,
        string currencyCode = "EUR",
        bool isActive = false)
    {
        var request = new { BillingPeriod = billingPeriod, Amount = amount, CurrencyCode = currencyCode, IsActive = isActive };
        var response = await Client.PostAsJsonAsync($"/plans/{planId}/pricing-tiers", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions))!;
    }

    public async Task<PlanResponse> AddPricingTierProviderConfigToPlanAsync(
        Guid planId,
        string billingPeriod = "Monthly",
        string provider = "Stripe",
        string externalProductId = "prod_123",
        string externalPriceId = "price_123",
        bool isActive = true)
    {
        var request = new { Provider = provider, ExternalProductId = externalProductId, ExternalPriceId = externalPriceId, IsActive = isActive };
        var response = await Client.PostAsJsonAsync($"/plans/{planId}/pricing-tiers/{billingPeriod}/provider-configurations", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions))!;
    }

    public async Task<PlanResponse> CreateCompletePlanAsync(
        string name = "Plan Completo",
        string description = "Plan con feature y provider")
    {
        var plan = await CreatePlanAsync(name, description);
        await AddFeatureToPlanAsync(plan.Id);
        await AddPricingTierToPlanAsync(plan.Id, isActive: true);
        return await AddPricingTierProviderConfigToPlanAsync(plan.Id);
    }
}
