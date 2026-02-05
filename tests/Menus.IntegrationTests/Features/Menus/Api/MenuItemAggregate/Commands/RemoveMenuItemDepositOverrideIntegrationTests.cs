namespace Menus.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemDepositOverrideIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    private async Task<MenuItemResponse> SetDepositOverrideAsync(
        Guid menuItemId,
        decimal depositAmount = 30.00m,
        int? minimumQuantityForDeposit = null)
    {
        var request = new SetMenuItemDepositOverride.Request(
            DepositAmount: depositAmount,
            MinimumQuantityForDeposit: minimumQuantityForDeposit
        );

        await Client.PutAsJsonAsync($"/menu-items/{menuItemId}/deposit-override", request);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItemId}");
        return (await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions))!;
    }

    #region Success Tests

    [Fact]
    public async Task RemoveDepositOverride_WithExistingOverride_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pulpo al Horno");
        await SetDepositOverrideAsync(menuItem.Id, depositAmount: 30.00m);

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/deposit-override");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveDepositOverride_WithExistingOverride_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Jamón Ibérico");
        await SetDepositOverrideAsync(menuItem.Id, depositAmount: 50.00m, minimumQuantityForDeposit: 4);

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/deposit-override");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.HasDepositOverride.Should().BeFalse();
        result.DepositOverride.Should().BeNull();
    }

    [Fact]
    public async Task RemoveDepositOverride_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla"
        );
        await SetDepositOverrideAsync(menuItem.Id, depositAmount: 30.00m);

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/deposit-override");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
    }

    [Fact]
    public async Task RemoveDepositOverride_ShouldPreservePriceOptions()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        await SetDepositOverrideAsync(menuItem.Id, depositAmount: 30.00m);

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/deposit-override");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().NotBeEmpty();
    }

    #endregion

    #region Idempotent Tests

    [Fact]
    public async Task RemoveDepositOverride_WithoutExistingOverride_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Test Item");

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/deposit-override");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveDepositOverride_CalledTwice_ShouldReturnNoContentBothTimes()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        await SetDepositOverrideAsync(menuItem.Id, depositAmount: 30.00m);

        // Act
        var firstResponse = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/deposit-override");
        var secondResponse = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/deposit-override");

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveDepositOverride_CalledTwice_ShouldProduceSameResult()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        await SetDepositOverrideAsync(menuItem.Id, depositAmount: 30.00m);

        // Act
        await Client.DeleteAsync($"/menu-items/{menuItem.Id}/deposit-override");
        await Client.DeleteAsync($"/menu-items/{menuItem.Id}/deposit-override");

        // Assert
        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.HasDepositOverride.Should().BeFalse();
        result.DepositOverride.Should().BeNull();
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task RemoveDepositOverride_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{nonExistentId}/deposit-override");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
