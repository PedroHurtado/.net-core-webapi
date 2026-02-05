namespace Menus.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class UpdateMenuIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    private static UpdateMenu.Request CreateValidRequest(
        string name = "Carta Actualizada",
        string? description = null,
        DateTime? effectiveFrom = null,
        DateTime? effectiveUntil = null,
        int displayOrder = 0)
    {
        return new UpdateMenu.Request(
            Name: name,
            Description: description,
            EffectiveFrom: effectiveFrom,
            EffectiveUntil: effectiveUntil,
            DisplayOrder: displayOrder
        );
    }

    #region Success Tests

    [Fact]
    public async Task Update_WithValidData_Returns204()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta Original");
        var request = CreateValidRequest(name: "Carta Modificada");

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldPersistChanges()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta Original");
        var request = CreateValidRequest(
            name: "Carta Actualizada",
            description: "Nueva descripción",
            displayOrder: 5
        );

        // Act
        await Client.PutAsJsonAsync($"/menus/{menu.Id}", request);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Name.Should().Be("Carta Actualizada");
        result.Description.Should().Be("Nueva descripción");
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task Update_WithEffectiveDates_ShouldPersistDates()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Menú Temporal");
        var effectiveFrom = new DateTime(2025, 6, 1);
        var effectiveUntil = new DateTime(2025, 8, 31);
        var request = CreateValidRequest(
            name: "Menú Verano",
            effectiveFrom: effectiveFrom,
            effectiveUntil: effectiveUntil
        );

        // Act
        await Client.PutAsJsonAsync($"/menus/{menu.Id}", request);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.EffectiveFrom.Should().Be(effectiveFrom);
        result.EffectiveUntil.Should().Be(effectiveUntil);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Update_WithNonExistentId_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Update_WithEmptyName_Returns422()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta Original");
        var request = CreateValidRequest(name: "");

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Update_WithNegativeDisplayOrder_Returns422()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta Original");
        var request = CreateValidRequest(displayOrder: -1);

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Update_WithInvalidDateRange_Returns422()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta Original");
        var request = CreateValidRequest(
            effectiveFrom: new DateTime(2025, 12, 31),
            effectiveUntil: new DateTime(2025, 12, 1)
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
