using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using webapi.infrastructure;
using webapi.features.ingredients.commands;

namespace WebApi.IntegrationTests;

public class CreateIngredientTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CreateIngredientTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remover el DbContext existente
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Agregar DbContext en memoria para tests
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateIngredient_WithValidData_ShouldReturnCreatedAndIngredient()
    {
        // Arrange
        var request = new CreateIngredient.Request(
            Name: "Tomate",
            Cost: 2.50m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/ingredientes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<CreateIngredient.Response>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Tomate");
        result.Cost.Should().Be(2.50m);
    }

    [Fact]
    public async Task CreateIngredient_WithValidData_ShouldPersistInDatabase()
    {
        // Arrange
        var request = new CreateIngredient.Request(
            Name: "Cebolla",
            Cost: 1.75m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/ingredientes", request);
        var result = await response.Content.ReadFromJsonAsync<CreateIngredient.Response>();

        // Assert
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var ingredient = await dbContext.Ingredients
            .FirstOrDefaultAsync(i => i.Id == result!.Id);

        ingredient.Should().NotBeNull();
        ingredient!.Name.Should().Be("Cebolla");
        ingredient.Cost.Should().Be(1.75m);
    }

    [Theory]
    [InlineData("", 2.50)]
    [InlineData(" ", 2.50)]
    public async Task CreateIngredient_WithInvalidName_ShouldReturnUnprocessableEntity(
        string invalidName, decimal cost)
    {
        // Arrange
        var request = new CreateIngredient.Request(
            Name: invalidName,
            Cost: cost
        );

        // Act
        var response = await _client.PostAsJsonAsync("/ingredientes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateIngredient_WithNameExceeding100Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = new CreateIngredient.Request(
            Name: new string('a', 101),
            Cost: 2.50m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/ingredientes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public async Task CreateIngredient_WithCostLessThanOrEqualToZero_ShouldReturnUnprocessableEntity(
        decimal invalidCost)
    {
        // Arrange
        var request = new CreateIngredient.Request(
            Name: "Tomate",
            Cost: invalidCost
        );

        // Act
        var response = await _client.PostAsJsonAsync("/ingredientes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Theory]
    [InlineData(10000)]
    [InlineData(10000.01)]
    [InlineData(99999)]
    public async Task CreateIngredient_WithCostGreaterThanOrEqualTo10000_ShouldReturnUnprocessableEntity(
        decimal invalidCost)
    {
        // Arrange
        var request = new CreateIngredient.Request(
            Name: "Tomate",
            Cost: invalidCost
        );

        // Act
        var response = await _client.PostAsJsonAsync("/ingredientes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateIngredient_WithMinimumValidCost_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateIngredient.Request(
            Name: "Ingrediente",
            Cost: 0.01m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/ingredientes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<CreateIngredient.Response>();
        result!.Cost.Should().Be(0.01m);
    }

    [Fact]
    public async Task CreateIngredient_WithMaximumValidCost_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateIngredient.Request(
            Name: "Ingrediente Premium",
            Cost: 9999.99m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/ingredientes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<CreateIngredient.Response>();
        result!.Cost.Should().Be(9999.99m);
    }

    [Fact]
    public async Task CreateIngredient_WithNameExactly100Characters_ShouldReturnCreated()
    {
        // Arrange
        var name = new string('a', 100);
        var request = new CreateIngredient.Request(
            Name: name,
            Cost: 2.50m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/ingredientes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<CreateIngredient.Response>();
        result!.Name.Should().Be(name);
    }

    [Fact]
    public async Task CreateIngredient_MultipleTimes_ShouldCreateMultipleIngredients()
    {
        // Arrange
        var request1 = new CreateIngredient.Request("Tomate", 2.50m);
        var request2 = new CreateIngredient.Request("Cebolla", 1.75m);
        var request3 = new CreateIngredient.Request("Ajo", 0.50m);

        // Act
        var response1 = await _client.PostAsJsonAsync("/ingredientes", request1);
        var response2 = await _client.PostAsJsonAsync("/ingredientes", request2);
        var response3 = await _client.PostAsJsonAsync("/ingredientes", request3);

        var result1 = await response1.Content.ReadFromJsonAsync<CreateIngredient.Response>();
        var result2 = await response2.Content.ReadFromJsonAsync<CreateIngredient.Response>();
        var result3 = await response3.Content.ReadFromJsonAsync<CreateIngredient.Response>();

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);
        response3.StatusCode.Should().Be(HttpStatusCode.Created);

        result1!.Id.Should().NotBe(result2!.Id);
        result2.Id.Should().NotBe(result3!.Id);
        result1.Id.Should().NotBe(result3.Id);

        // Verificar en base de datos
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var ingredients = await dbContext.Ingredients.ToListAsync();
        ingredients.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task CreateIngredient_WithMultipleValidationErrors_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = new CreateIngredient.Request(
            Name: "",
            Cost: -5m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/ingredientes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}