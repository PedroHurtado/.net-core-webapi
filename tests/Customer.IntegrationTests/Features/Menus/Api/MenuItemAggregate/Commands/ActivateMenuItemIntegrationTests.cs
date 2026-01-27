namespace Customer.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class ActivateMenuItemIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task ActivateMenuItem_WithInactiveMenuItem_ShouldReturnOkAndActivatedResponse()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pulpo al Horno");

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(menuItem.Id);
        result.Name.Should().Be("Pulpo al Horno");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateMenuItem_WithInactiveMenuItem_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Jamón Ibérico");

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateMenuItem_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla"
        );

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateMenuItem_ShouldReturnCanBeOrderedTrue()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.CanBeOrdered.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateMenuItem_ShouldPreservePriceOptions()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            priceOptions: [
                new(PortionType.Small, 5.00m),
                new(PortionType.Full, 10.00m)
            ]
        );

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.PriceOptions.Should().HaveCount(2);
    }

    [Fact]
    public async Task ActivateMenuItem_WithMultiplePriceOptions_ShouldActivate()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            priceOptions: [
                new(PortionType.Small, 3.50m),
                new(PortionType.Half, 7.00m),
                new(PortionType.Full, 14.00m)
            ]
        );

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.IsActive.Should().BeTrue();
        result.PriceOptions.Should().HaveCount(3);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task ActivateMenuItem_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/menu-items/{nonExistentId}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task ActivateMenuItem_WithActiveMenuItem_ShouldReturnConflict()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Item to Activate Twice");
        await Client.PostAsync($"/menu-items/{menuItem.Id}/activate", null);

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ActivateMenuItem_WhenCalledTwice_ShouldReturnConflictOnSecondCall()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var firstResponse = await Client.PostAsync($"/menu-items/{menuItem.Id}/activate", null);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var secondResponse = await Client.PostAsync($"/menu-items/{menuItem.Id}/activate", null);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ActivateMenuItem_WithAllInactivePriceOptions_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            priceOptions: [new(PortionType.Full, 10.00m, IsActive: false)]
        );

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
