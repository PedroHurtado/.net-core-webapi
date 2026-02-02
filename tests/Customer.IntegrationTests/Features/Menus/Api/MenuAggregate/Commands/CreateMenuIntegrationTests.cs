namespace Customer.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class CreateMenuIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    private static CreateMenu.Request CreateValidRequest(
        string name = "Carta Principal",
        string? description = null,
        DateTime? effectiveFrom = null,
        DateTime? effectiveUntil = null)
    {
        return new CreateMenu.Request(
            Name: name,
            Description: description,
            EffectiveFrom: effectiveFrom,
            EffectiveUntil: effectiveUntil
        );
    }

    #region Success Tests

    [Fact]
    public async Task Create_WithValidData_Returns201()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var response = await Client.PostAsJsonAsync("/menus", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsMenuResponse()
    {
        // Arrange
        var request = CreateValidRequest(
            name: "Carta de Vinos",
            description: "Selección de vinos"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/menus", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Carta de Vinos");
        result.Description.Should().Be("Selección de vinos");
        result.IsActive.Should().BeFalse();
        result.DisplayOrder.Should().Be(0);
        result.Categories.Should().BeEmpty();
        result.DepositPolicy.Should().BeNull();
    }

    [Fact]
    public async Task Create_WithEffectiveDates_ReturnsMenuWithDates()
    {
        // Arrange
        var effectiveFrom = new DateTime(2025, 12, 1);
        var effectiveUntil = new DateTime(2025, 12, 31);
        var request = CreateValidRequest(
            name: "Menú Navidad",
            effectiveFrom: effectiveFrom,
            effectiveUntil: effectiveUntil
        );

        // Act
        var response = await Client.PostAsJsonAsync("/menus", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.EffectiveFrom.Should().Be(effectiveFrom);
        result.EffectiveUntil.Should().Be(effectiveUntil);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldPersistMenu()
    {
        // Arrange
        var request = CreateValidRequest(name: "Carta Persistida");

        // Act
        var createResponse = await Client.PostAsJsonAsync("/menus", request);
        var created = await createResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);
        result!.Name.Should().Be("Carta Persistida");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Create_WithEmptyName_Returns422()
    {
        // Arrange
        var request = CreateValidRequest(name: "");

        // Act
        var response = await Client.PostAsJsonAsync("/menus", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithInvalidDateRange_Returns422()
    {
        // Arrange
        var request = CreateValidRequest(
            name: "Menú Inválido",
            effectiveFrom: new DateTime(2025, 12, 31),
            effectiveUntil: new DateTime(2025, 12, 1)
        );

        // Act
        var response = await Client.PostAsJsonAsync("/menus", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
