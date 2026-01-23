using System.Net.Http.Json;
using Customer.Features.Menus.Api.Commands.Allergens;
using Customer.Infrastructure;
using Fudie.Firestore.EntityFrameworkCore.Infrastructure;
using Fudie.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Customer.IntegrationTests.Fixtures;

public class CustomerWebApplicationFixture : IClassFixture<WebApplicationFactory<Program>>
{
    private const string EmulatorHost = "127.0.0.1:8080";

    private readonly WebApplicationFactory<Program> _factory;

    public HttpClient Client { get; }
    public IServiceProvider Services => _factory.Services;

    public CustomerWebApplicationFixture(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", EmulatorHost);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CustomerDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var tenantId = Guid.NewGuid();

                services.AddDbContext<CustomerDbContext>(options =>
                {
                    options.UseFirestore("demo-project");
                }, ServiceLifetime.Scoped);

                services.AddScoped(typeof(Guid), _ => tenantId);

                services.AddScoped<IChangeTracker>(sp => sp.GetRequiredService<CustomerDbContext>());
                services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CustomerDbContext>());
                services.AddScoped<IQuery>(sp => sp.GetRequiredService<CustomerDbContext>());
                services.AddScoped<IEntityLookup>(sp => sp.GetRequiredService<CustomerDbContext>());
            });
        });

        Client = _factory.CreateClient();
    }

    public T GetService<T>() where T : notnull
    {
        using var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    public async Task<T> ExecuteWithDbContext<T>(Func<CustomerDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        return await action(dbContext);
    }

    public async Task<CreateAllergen.Response> CreateAllergenAsync(
        string? code = null,
        string name = "Test Allergen",
        string? iconUrl = null,
        bool isActive = true,
        int displayOrder = 0)
    {
        code ??= "TEST_" + Guid.NewGuid().ToString("N")[..8].ToUpper();

        var request = new CreateAllergen.Request(code, name, iconUrl, isActive, displayOrder);
        var response = await Client.PostAsJsonAsync("/allergens", request);

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateAllergen.Response>())!;
    }
}
