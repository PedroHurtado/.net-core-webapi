namespace Customer.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Queries;

public class GetMenuItemIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task GetMenuItem_WithExistingId_ShouldReturnOkAndMenuItem()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pulpo al Horno");

        // Act
        var response = await Client.GetAsync($"/menu-items/{menuItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(menuItem.Id);
        result.Name.Should().Be("Pulpo al Horno");
    }

    [Fact]
    public async Task GetMenuItem_WithAllFields_ShouldReturnCompleteResponse()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Jamón Ibérico",
            description: "Jamón de bellota 100%",
            imageUrl: "https://example.com/jamon.jpg",
            displayOrder: 5,
            isHighRiskItem: true,
            requiresAdvanceOrder: true,
            minimumAdvanceOrderQuantity: 4,
            isAlwaysAvailable: false,
            availableDays: [DayOfWeek.Friday, DayOfWeek.Saturday],
            allergenNotes: "Puede contener trazas",
            priceOptions: [
                new(PortionType.Small, 3.50m),
                new(PortionType.Half, 7.00m),
                new(PortionType.Full, 14.00m)
            ]
        );

        // Act
        var response = await Client.GetAsync($"/menu-items/{menuItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(menuItem.Id);
        result.Name.Should().Be("Jamón Ibérico");
        result.Description.Should().Be("Jamón de bellota 100%");
        result.ImageUrl.Should().Be("https://example.com/jamon.jpg");
        result.DisplayOrder.Should().Be(5);
        result.IsHighRiskItem.Should().BeTrue();
        result.RequiresAdvanceOrder.Should().BeTrue();
        result.MinimumAdvanceOrderQuantity.Should().Be(4);
        result.IsAlwaysAvailable.Should().BeFalse();
        result.AvailableDays.Should().HaveCount(2);
        result.AllergenNotes.Should().Be("Puede contener trazas");
        result.PriceOptions.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetMenuItem_WithNullOptionalFields_ShouldReturnNullValues()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Simple Item",
            description: null,
            imageUrl: null,
            allergenNotes: null
        );

        // Act
        var response = await Client.GetAsync($"/menu-items/{menuItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.Description.Should().BeNull();
        result.ImageUrl.Should().BeNull();
        result.AllergenNotes.Should().BeNull();
    }

    [Fact]
    public async Task GetMenuItem_WithDefaultValues_ShouldReturnDefaults()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        // Act
        var response = await Client.GetAsync($"/menu-items/{menuItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.IsActive.Should().BeFalse();
        result.IsAvailable.Should().BeTrue();
        result.IsAlwaysAvailable.Should().BeTrue();
        result.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public async Task GetMenuItem_ShouldReturnComputedProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(isAlwaysAvailable: true);

        // Act
        var response = await Client.GetAsync($"/menu-items/{menuItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.IsAvailableToday.Should().BeTrue();
        result.CanBeOrdered.Should().BeFalse();
        result.HasDepositOverride.Should().BeFalse();
    }

    [Fact]
    public async Task GetMenuItem_WithMultiplePriceOptions_ShouldReturnAllOptions()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            priceOptions: [
                new(PortionType.Small, 4.00m),
                new(PortionType.Half, 7.50m),
                new(PortionType.Full, 12.00m)
            ]
        );

        // Act
        var response = await Client.GetAsync($"/menu-items/{menuItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.PriceOptions.Should().HaveCount(3);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Small && p.Price == 4.00m);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.Price == 7.50m);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full && p.Price == 12.00m);
    }

    [Fact]
    public async Task GetMenuItem_WithMarketPriceOption_ShouldReturnCorrectPriceOption()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            priceOptions: [new(PortionType.MarketPrice, null)]
        );

        // Act
        var response = await Client.GetAsync($"/menu-items/{menuItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.PriceOptions.Should().HaveCount(1);
        var priceOption = result.PriceOptions.First();
        priceOption.PortionType.Should().Be(PortionType.MarketPrice);
        priceOption.Price.Should().BeNull();
        priceOption.RequiresMarketPrice.Should().BeTrue();
    }

    [Fact]
    public async Task GetMenuItem_WithAvailableDays_ShouldReturnDays()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            isAlwaysAvailable: false,
            availableDays: [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday]
        );

        // Act
        var response = await Client.GetAsync($"/menu-items/{menuItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.IsAlwaysAvailable.Should().BeFalse();
        result.AvailableDays.Should().HaveCount(3);
        result.AvailableDays.Should().Contain(DayOfWeek.Monday);
        result.AvailableDays.Should().Contain(DayOfWeek.Wednesday);
        result.AvailableDays.Should().Contain(DayOfWeek.Friday);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task GetMenuItem_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/menu-items/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
