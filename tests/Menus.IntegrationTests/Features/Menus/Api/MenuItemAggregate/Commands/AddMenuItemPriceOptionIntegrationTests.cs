namespace Menus.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class AddMenuItemPriceOptionIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    private static AddMenuItemPriceOption.Request CreateValidRequest(
        PortionType portionType = PortionType.Half,
        decimal? price = 15.00m,
        bool isActive = true)
    {
        return new AddMenuItemPriceOption.Request(
            PortionType: portionType,
            Price: price,
            IsActive: isActive
        );
    }

    #region Success Tests

    [Fact]
    public async Task AddPriceOption_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pulpo al Horno");
        var request = CreateValidRequest(portionType: PortionType.Half, price: 15.00m);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddPriceOption_WithValidData_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Jamón Ibérico");
        var request = CreateValidRequest(portionType: PortionType.Half, price: 18.50m);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().HaveCount(2);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.Price == 18.50m);
    }

    [Fact]
    public async Task AddPriceOption_WithSmallPortionType_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(portionType: PortionType.Small, price: 8.00m);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Small && p.Price == 8.00m);
    }

    [Fact]
    public async Task AddPriceOption_WithMarketPriceAndNoPrice_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(portionType: PortionType.MarketPrice, price: null);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().Contain(p => p.PortionType == PortionType.MarketPrice && p.RequiresMarketPrice);
    }

    [Fact]
    public async Task AddPriceOption_WithIsActiveFalse_ShouldPersistAsInactive()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(portionType: PortionType.Half, price: 15.00m, isActive: false);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && !p.IsActive);
    }

    [Fact]
    public async Task AddPriceOption_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla"
        );
        var request = CreateValidRequest(portionType: PortionType.Half, price: 15.00m);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
    }

    [Fact]
    public async Task AddPriceOption_ShouldPreserveExistingPriceOptions()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var existingOption = menuItem.PriceOptions.First();
        var request = CreateValidRequest(portionType: PortionType.Half, price: 15.00m);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().Contain(p =>
            p.PortionType == existingOption.PortionType &&
            p.Price == existingOption.Price);
    }

    [Fact]
    public async Task AddPriceOption_AddingMultiplePriceOptions_ShouldPersistAll()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var halfRequest = CreateValidRequest(portionType: PortionType.Half, price: 15.00m);
        var smallRequest = CreateValidRequest(portionType: PortionType.Small, price: 8.00m);

        // Act
        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", halfRequest);
        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", smallRequest);

        // Assert
        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().HaveCount(3);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Small);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task AddPriceOption_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{nonExistentId}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task AddPriceOption_WithDuplicatePortionType_ShouldReturnConflict()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(portionType: PortionType.Full, price: 25.00m);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task AddPriceOption_WithNegativePrice_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(portionType: PortionType.Half, price: -5.00m);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AddPriceOption_WithNullPriceForFixedPortionType_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(portionType: PortionType.Half, price: null);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AddPriceOption_WithZeroPrice_ShouldSucceed()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(portionType: PortionType.Half, price: 0m);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/price-options", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion
}
