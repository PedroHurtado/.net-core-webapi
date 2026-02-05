namespace Menus.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class SetMenuItemDepositOverrideIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    private static SetMenuItemDepositOverride.Request CreateValidRequest(
        decimal depositAmount = 30.00m,
        int? minimumQuantityForDeposit = null)
    {
        return new SetMenuItemDepositOverride.Request(
            DepositAmount: depositAmount,
            MinimumQuantityForDeposit: minimumQuantityForDeposit
        );
    }

    #region Success Tests

    [Fact]
    public async Task SetDepositOverride_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pulpo al Horno");
        var request = CreateValidRequest(depositAmount: 30.00m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/deposit-override", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetDepositOverride_WithValidData_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Jamón Ibérico");
        var request = CreateValidRequest(depositAmount: 50.00m, minimumQuantityForDeposit: 4);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/deposit-override", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.HasDepositOverride.Should().BeTrue();
        result.DepositOverride.Should().NotBeNull();
        result.DepositOverride!.DepositAmount.Should().Be(50.00m);
        result.DepositOverride.MinimumQuantityForDeposit.Should().Be(4);
    }

    [Fact]
    public async Task SetDepositOverride_WithoutMinimumQuantity_ShouldApplyToAllQuantities()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(depositAmount: 25.00m, minimumQuantityForDeposit: null);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/deposit-override", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.DepositOverride!.AppliesToAllQuantities.Should().BeTrue();
    }

    [Fact]
    public async Task SetDepositOverride_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla"
        );
        var request = CreateValidRequest(depositAmount: 30.00m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/deposit-override", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
    }

    [Fact]
    public async Task SetDepositOverride_ShouldReplaceExistingOverride()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var firstRequest = CreateValidRequest(depositAmount: 20.00m, minimumQuantityForDeposit: 2);
        await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/deposit-override", firstRequest);

        var secondRequest = CreateValidRequest(depositAmount: 50.00m, minimumQuantityForDeposit: 5);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/deposit-override", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.DepositOverride!.DepositAmount.Should().Be(50.00m);
        result.DepositOverride.MinimumQuantityForDeposit.Should().Be(5);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task SetDepositOverride_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = CreateValidRequest(depositAmount: 30.00m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{nonExistentId}/deposit-override", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task SetDepositOverride_WithZeroDepositAmount_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(depositAmount: 0m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/deposit-override", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetDepositOverride_WithNegativeDepositAmount_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(depositAmount: -10.00m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/deposit-override", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetDepositOverride_WithZeroMinimumQuantity_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(depositAmount: 30.00m, minimumQuantityForDeposit: 0);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/deposit-override", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetDepositOverride_WithDepositAmountExceedingMax_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(depositAmount: 10001m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/deposit-override", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetDepositOverride_WithMinimumQuantityExceedingMax_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(depositAmount: 30.00m, minimumQuantityForDeposit: 101);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/deposit-override", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
