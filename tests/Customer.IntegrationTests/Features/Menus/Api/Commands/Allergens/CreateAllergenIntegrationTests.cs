using System.Net;
using System.Net.Http.Json;
using Customer.Features.Menus.Api.Commands.Allergens;
using Customer.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer.IntegrationTests.Features.Menus.Api.Commands.Allergens;

public class CreateAllergenIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
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
        var response = await Client.PostAsJsonAsync("/allergens", request);

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
        var response = await Client.PostAsJsonAsync("/allergens", request);

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
        var response = await Client.PostAsJsonAsync("/allergens", request);
        var result = await response.Content.ReadFromJsonAsync<CreateAllergen.Response>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();

        // Verificar en base de datos
        var allergen = await ExecuteWithDbContext(db => db.Allergens.FindAsync(result!.Id).AsTask());

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
        var response = await Client.PostAsJsonAsync("/allergens", request);

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
        var response = await Client.PostAsJsonAsync("/allergens", request);

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
        var response = await Client.PostAsJsonAsync("/allergens", request);

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
        var response = await Client.PostAsJsonAsync("/allergens", request);

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
        var response = await Client.PostAsJsonAsync("/allergens", request);

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
        var response = await Client.PostAsJsonAsync("/allergens", request);

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
        var response = await Client.PostAsJsonAsync("/allergens", request);

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
        var response = await Client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public async Task CreateAllergen_WithCodeExactly20Characters_ShouldReturnCreated()
    {
        // Arrange
        var prefix = Guid.NewGuid().ToString("N")[..12].ToUpper();
        var code = prefix + new string('A', 8); // Total: 20 caracteres
        var request = new CreateAllergen.Request(
            Code: code,
            Name: "Gluten"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/allergens", request);

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
        var response = await Client.PostAsJsonAsync("/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateAllergen.Response>();
        result!.Name.Should().Be(name);
    }

    #endregion
}
