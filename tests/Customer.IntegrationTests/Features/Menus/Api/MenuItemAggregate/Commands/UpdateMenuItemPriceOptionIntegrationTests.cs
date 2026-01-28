namespace Customer.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class UpdateMenuItemPriceOptionIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    private static UpdateMenuItemPriceOption.Request CreateValidRequest(
        decimal? price = 25.00m,
        bool isActive = true)
    {
        return new UpdateMenuItemPriceOption.Request(
            Price: price,
            IsActive: isActive
        );
    }

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
    public async Task UpdatePriceOption_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pulpo al Horno");
        var request = CreateValidRequest(price: 30.00m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdatePriceOption_WithValidData_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Jamón Ibérico");
        var request = CreateValidRequest(price: 35.50m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full && p.Price == 35.50m);
    }

    [Fact]
    public async Task UpdatePriceOption_DeactivatingWithOtherActiveOptions_ShouldSucceed()
    {
        // Arrange
        var menuItem = await CreateMenuItemWithMultiplePriceOptionsAsync();
        var request = CreateValidRequest(price: 22.00m, isActive: false);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.First(p => p.PortionType == PortionType.Full).IsActive.Should().BeFalse();
        result.PriceOptions.First(p => p.PortionType == PortionType.Half).IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePriceOption_ActivatingInactiveOption_ShouldSucceed()
    {
        // Arrange
        var menuItem = await CreateMenuItemWithMultiplePriceOptionsAsync();
        await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", CreateValidRequest(price: 22.00m, isActive: false));
        var request = CreateValidRequest(price: 22.00m, isActive: true);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.First(p => p.PortionType == PortionType.Full).IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePriceOption_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla"
        );
        var request = CreateValidRequest(price: 30.00m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
    }

    [Fact]
    public async Task UpdatePriceOption_ShouldPreserveOtherPriceOptions()
    {
        // Arrange
        var menuItem = await CreateMenuItemWithMultiplePriceOptionsAsync();
        var request = CreateValidRequest(price: 30.00m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().HaveCount(2);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.Price == 15.00m);
    }

    [Fact]
    public async Task UpdatePriceOption_WithZeroPrice_ShouldSucceed()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(price: 0m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.First(p => p.PortionType == PortionType.Full).Price.Should().Be(0m);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task UpdatePriceOption_WithNonExistentMenuItemId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{nonExistentId}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePriceOption_WithNonExistentPortionType_ShouldReturnNotFound()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Half}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task UpdatePriceOption_DeactivatingLastActiveOption_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(price: 22.00m, isActive: false);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdatePriceOption_WithNegativePrice_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(price: -5.00m);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdatePriceOption_WithNullPriceForFixedPortionType_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(price: null);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdatePriceOption_DeactivatingLastActiveWhenOthersInactive_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemWithMultiplePriceOptionsAsync();
        var deactivateResponse = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Half}", CreateValidRequest(price: 15.00m, isActive: false));
        deactivateResponse.EnsureSuccessStatusCode();
        var request = CreateValidRequest(price: 22.00m, isActive: false);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/price-options/{PortionType.Full}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
