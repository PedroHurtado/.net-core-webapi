namespace Menus.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class DeactivateMenuItemIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    private async Task<MenuItemResponse> ActivateMenuItemAsync(Guid id)
    {
        var response = await Client.PostAsync($"/menu-items/{id}/activate", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions))!;
    }

    #region Success Tests

    [Fact]
    public async Task DeactivateMenuItem_WithActiveMenuItem_ShouldReturnOkAndDeactivatedResponse()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pulpo al Horno");
        await ActivateMenuItemAsync(menuItem.Id);

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(menuItem.Id);
        result.Name.Should().Be("Pulpo al Horno");
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateMenuItem_WithActiveMenuItem_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Jamón Ibérico");
        await ActivateMenuItemAsync(menuItem.Id);

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateMenuItem_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla"
        );
        await ActivateMenuItemAsync(menuItem.Id);

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateMenuItem_ShouldReturnCanBeOrderedFalse()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        await ActivateMenuItemAsync(menuItem.Id);

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.CanBeOrdered.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateMenuItem_ShouldPreservePriceOptions()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            priceOptions: [
                new(PortionType.Small, 5.00m),
                new(PortionType.Full, 10.00m)
            ]
        );
        await ActivateMenuItemAsync(menuItem.Id);

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.PriceOptions.Should().HaveCount(2);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task DeactivateMenuItem_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/menu-items/{nonExistentId}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task DeactivateMenuItem_WithInactiveMenuItem_ShouldReturnConflict()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Inactive Item");

        // Act
        var response = await Client.PostAsync($"/menu-items/{menuItem.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeactivateMenuItem_WhenCalledTwice_ShouldReturnConflictOnSecondCall()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        await ActivateMenuItemAsync(menuItem.Id);

        var firstResponse = await Client.PostAsync($"/menu-items/{menuItem.Id}/deactivate", null);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var secondResponse = await Client.PostAsync($"/menu-items/{menuItem.Id}/deactivate", null);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion
}
