using System.Net;
using System.Net.Http.Json;
using Customer.Features.Menus.Api.Commands.Allergens;
using Customer.Infrastructure;
using FluentAssertions;
using Fudie.Firestore.EntityFrameworkCore.Infrastructure;
using Fudie.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Customer.IntegrationTests.Features.Menus.Api.Commands.Allergens;

public class CreateAllergenIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string EmulatorHost = "127.0.0.1:8080";

    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CreateAllergenIntegrationTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", EmulatorHost);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remover el DbContext existente
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CustomerDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Tenant ID para tests
                var tenantId = Guid.NewGuid();

                // Agregar DbContext con Firestore emulator
                services.AddDbContext<CustomerDbContext>(options =>
                {
                    options.UseFirestore("demo-project");
                }, ServiceLifetime.Scoped);

                // Re-registrar el tenantId
                services.AddScoped(typeof(Guid), _ => tenantId);

                // Re-registrar las interfaces que usa CustomerDbContext
                services.AddScoped<IChangeTracker>(sp => sp.GetRequiredService<CustomerDbContext>());
                services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CustomerDbContext>());
                services.AddScoped<IQuery>(sp => sp.GetRequiredService<CustomerDbContext>());
                services.AddScoped<IEntityLookup>(sp => sp.GetRequiredService<CustomerDbContext>());
            });
        });

        _client = _factory.CreateClient();
    }

    #region Success Tests

    [Fact]
    public async Task CreateAllergen_WithValidData_ShouldReturnCreatedAndAllergen()
    {
        // Arrange
        var code = "GLUTEN_" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        var request = new CreateAllergen.Request(
            Code: code,
            Name: "Gluten"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateAllergen.Response>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(code);
        result.Name.Should().Be("Gluten");
    }

    [Fact]
    public async Task CreateAllergen_WithAllFields_ShouldReturnCompleteResponse()
    {
        // Arrange
        var code = "LACTEOS_" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        var request = new CreateAllergen.Request(
            Code: code,
            Name: "Lácteos",
            IconUrl: "https://example.com/lacteos.png",
            IsActive: false,
            DisplayOrder: 5
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateAllergen.Response>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(code);
        result.Name.Should().Be("Lácteos");
        result.IconUrl.Should().Be("https://example.com/lacteos.png");
        result.IsActive.Should().BeFalse();
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task CreateAllergen_WithValidData_ShouldPersistInDatabase()
    {
        // Arrange
        var code = "NUECES_" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        var request = new CreateAllergen.Request(
            Code: code,
            Name: "Nueces"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);
        var result = await response.Content.ReadFromJsonAsync<CreateAllergen.Response>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();

        // Verificar en base de datos
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        var allergen = await dbContext.Allergens.FindAsync(result!.Id);

        allergen.Should().NotBeNull();
        allergen!.Name.Should().Be("Nueces");
    }

    [Fact]
    public async Task CreateAllergen_WithDefaultValues_ShouldApplyDefaults()
    {
        // Arrange
        var code = "SOJA_" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        var request = new CreateAllergen.Request(code, "Soja");

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateAllergen.Response>();
        result!.IconUrl.Should().BeNull();
        result.IsActive.Should().BeTrue();
        result.DisplayOrder.Should().Be(0);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAllergen_WithInvalidCode_ShouldReturnUnprocessableEntity(string invalidCode)
    {
        // Arrange
        var request = new CreateAllergen.Request(
            Code: invalidCode,
            Name: "Gluten"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Theory]
    [InlineData("gluten")]
    [InlineData("Gluten")]
    [InlineData("GLUTEN-1")]
    [InlineData("GLUTEN 1")]
    public async Task CreateAllergen_WithInvalidCodeFormat_ShouldReturnUnprocessableEntity(string invalidCode)
    {
        // Arrange
        var request = new CreateAllergen.Request(
            Code: invalidCode,
            Name: "Gluten"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateAllergen_WithCodeExceeding20Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = new CreateAllergen.Request(
            Code: new string('A', 21),
            Name: "Gluten"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAllergen_WithInvalidName_ShouldReturnUnprocessableEntity(string invalidName)
    {
        // Arrange
        var code = "GLUTEN_" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        var request = new CreateAllergen.Request(
            Code: code,
            Name: invalidName
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateAllergen_WithNameExceeding100Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var code = "GLUTEN_" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        var request = new CreateAllergen.Request(
            Code: code,
            Name: new string('a', 101)
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateAllergen_WithInvalidIconUrl_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var code = "GLUTEN_" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        var request = new CreateAllergen.Request(
            Code: code,
            Name: "Gluten",
            IconUrl: "not-a-url"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateAllergen_WithNegativeDisplayOrder_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var code = "GLUTEN_" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        var request = new CreateAllergen.Request(
            Code: code,
            Name: "Gluten",
            DisplayOrder: -1
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public async Task CreateAllergen_WithCodeExactly20Characters_ShouldReturnCreated()
    {
        // Arrange
        var code = new string('A', 20);
        var request = new CreateAllergen.Request(
            Code: code,
            Name: "Gluten"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateAllergen.Response>();
        result!.Id.Should().Be(code);
    }

    [Fact]
    public async Task CreateAllergen_WithNameExactly100Characters_ShouldReturnCreated()
    {
        // Arrange
        var code = "GLUTEN_" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        var name = new string('a', 100);
        var request = new CreateAllergen.Request(
            Code: code,
            Name: name
        );

        // Act
        var response = await _client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateAllergen.Response>();
        result!.Name.Should().Be(name);
    }

    #endregion
}
