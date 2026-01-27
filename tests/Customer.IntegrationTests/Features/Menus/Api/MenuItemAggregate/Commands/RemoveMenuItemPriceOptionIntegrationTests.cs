namespace Customer.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemPriceOptionIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    private async Task<MenuItemResponse> CreateMenuItemWithMultiplePriceOptionsAsync()
    {
        return await CreateMenuItemAsync(
            name: "Test MenuItem",
            priceOptions:
            [
                new CreateMenuItem.CreatePriceOptionRequest(PortionType.Full, 22.00m),
                new CreateMenuItem.CreatePriceOptionRequest(PortionType.Half, 15.00m)
            ]
        );
    }

    #region Success Tests

    [Fact]
    public async Task RemovePriceOption_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemWithMultiplePriceOptionsAsync();

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Half}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemovePriceOption_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla",
            priceOptions:
            [
                new CreateMenuItem.CreatePriceOptionRequest(PortionType.Full, 22.00m),
                new CreateMenuItem.CreatePriceOptionRequest(PortionType.Half, 15.00m)
            ]
        );

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Half}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task RemovePriceOption_WithNonExistentMenuItemId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{nonExistentId}/price-options/{PortionType.Full}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemovePriceOption_WithNonExistentPortionType_ShouldReturnNotFound()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Half}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task RemovePriceOption_RemovingLastPriceOption_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
