using System.Text.Json;
using System.Text.Json.Serialization;

namespace Customer.IntegrationTests.Fixtures;

public class CustomerWebApplicationFixture : IClassFixture<WebApplicationFactory<Program>>
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

    public async Task<MenuItemResponse> CreateMenuItemAsync(
        string name = "Test MenuItem",
        string? description = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isHighRiskItem = false,
        bool requiresAdvanceOrder = false,
        int? minimumAdvanceOrderQuantity = null,
        bool isAlwaysAvailable = true,
        DayOfWeek[]? availableDays = null,
        string? allergenNotes = null,
        CreateMenuItem.CreatePriceOptionRequest[]? priceOptions = null)
    {
        var request = new CreateMenuItem.Request(
            Name: name,
            Description: description,
            ImageUrl: imageUrl,
            DisplayOrder: displayOrder,
            IsHighRiskItem: isHighRiskItem,
            RequiresAdvanceOrder: requiresAdvanceOrder,
            MinimumAdvanceOrderQuantity: minimumAdvanceOrderQuantity,
            IsAlwaysAvailable: isAlwaysAvailable,
            AvailableDays: availableDays ?? [],
            AllergenNotes: allergenNotes,
            PriceOptions: priceOptions ?? [new(PortionType.Full, 22.00m)]
        );

        var response = await Client.PostAsJsonAsync("/menu-items", request);

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions))!;
    }

    public async Task<MenuResponse> CreateMenuAsync(
        string name = "Test Menu",
        string? description = null,
        DateTime? effectiveFrom = null,
        DateTime? effectiveUntil = null)
    {
        var request = new CreateMenu.Request(
            Name: name,
            Description: description,
            EffectiveFrom: effectiveFrom,
            EffectiveUntil: effectiveUntil
        );

        var response = await Client.PostAsJsonAsync("/menus", request);

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions))!;
    }

    public async Task<MenuResponse> CreateMenuWithCategoryAsync(
        string menuName = "Test Menu",
        string categoryName = "Test Category")
    {
        var menu = await CreateMenuAsync(name: menuName);

        var categoryRequest = new AddMenuCategory.Request(
            Name: categoryName,
            Description: null,
            DisplayOrder: 0
        );

        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", categoryRequest);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions))!;
    }

    public async Task<MenuResponse> CreateMenuWithCategoryAndItemAsync(
        string menuName = "Test Menu",
        string categoryName = "Test Category",
        string menuItemName = "Test Item")
    {
        var menuItem = await CreateMenuItemAsync(name: menuItemName);
        var menu = await CreateMenuWithCategoryAsync(menuName: menuName, categoryName: categoryName);

        var categoryId = menu.Categories.First().Id;
        var addItemRequest = new AddItemToCategory.Request(
            MenuItemId: menuItem.Id,
            DisplayOrder: 0,
            PriceOverrides: null
        );

        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", addItemRequest);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions))!;
    }

    public async Task<MenuResponse> CreateActiveMenuAsync(
        string menuName = "Test Menu",
        string categoryName = "Test Category",
        string menuItemName = "Test Item")
    {
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: menuName,
            categoryName: categoryName,
            menuItemName: menuItemName
        );

        var response = await Client.PostAsync($"/menus/{menu.Id}/activate", null);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions))!;
    }
}
