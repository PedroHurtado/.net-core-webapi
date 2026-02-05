namespace Menus.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class MarkMenuItemAsUnavailableIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task MarkAsUnavailable_WithAvailableMenuItem_ShouldReturnOkAndUnavailableResponse()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pulpo al Horno");

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/mark-unavailable", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(menuItem.Id);
        result.Name.Should().Be("Pulpo al Horno");
        result.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsUnavailable_WithAvailableMenuItem_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Jamón Ibérico");

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/mark-unavailable", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsUnavailable_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla"
        );

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/mark-unavailable", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsUnavailable_ShouldReturnCanBeOrderedFalse()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/mark-unavailable", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.IsAvailable.Should().BeFalse();
        result.CanBeOrdered.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsUnavailable_ShouldPreservePriceOptions()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            priceOptions: [
                new(PortionType.Small, 5.00m),
                new(PortionType.Full, 10.00m)
            ]
        );

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/mark-unavailable", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.PriceOptions.Should().HaveCount(2);
    }

    #endregion

    #region Idempotent Tests

    [Fact]
    public async Task MarkAsUnavailable_WhenCalledTwice_ShouldSucceedBothTimes()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        // Act
        var firstResponse = await Client.PostAsync($"/menu-items/{menuItem.Id}/mark-unavailable", null);
        var secondResponse = await Client.PostAsync($"/menu-items/{menuItem.Id}/mark-unavailable", null);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await secondResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsUnavailable_WhenCalledMultipleTimes_ShouldAlwaysReturnUnavailable()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        // Act & Assert
        for (int i = 0; i < 3; i++)
        {
            var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/mark-unavailable", null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
            result!.IsAvailable.Should().BeFalse();
        }
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task MarkAsUnavailable_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/menu-items/{nonExistentId}/mark-unavailable", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
