namespace Customer.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class CreateMenuItemIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    private static CreateMenuItem.Request CreateValidRequest(
        string name = "Pulpo al Horno",
        string? description = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isHighRiskItem = false,
        bool requiresAdvanceOrder = false,
        int? minimumAdvanceOrderQuantity = null,
        bool isAlwaysAvailable = true,
        DayOfWeek[]? availableDays = null,
        string? allergenNotes = null,
        CreateMenuItem.CreatePriceOptionRequest[]? priceOptions = null)
    {
        return new CreateMenuItem.Request(
            Name: name,
            Description: description,
            ImageUrl: imageUrl,
            DisplayOrder: displayOrder,
            IsHighRiskItem: isHighRiskItem,
            RequiresAdvanceOrder: requiresAdvanceOrder,
            MinimumAdvanceOrderQuantity: minimumAdvanceOrderQuantity,
            IsAlwaysAvailable: isAlwaysAvailable,
            AvailableDays: availableDays ?? [],
            AllergenNotes: allergenNotes,
            PriceOptions: priceOptions ?? [new(PortionType.Full, 22.00m)]
        );
    }

    #region Success Tests

    [Fact]
    public async Task CreateMenuItem_WithValidData_ShouldReturnCreatedAndMenuItem()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Pulpo al Horno");
        result.IsActive.Should().BeFalse();
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task CreateMenuItem_WithAllFields_ShouldReturnCompleteResponse()
    {
        // Arrange
        var request = CreateValidRequest(
            name: "Jamón Ibérico",
            description: "Jamón de bellota 100%",
            imageUrl: "https://example.com/jamon.jpg",
            displayOrder: 5,
            isHighRiskItem: true,
            requiresAdvanceOrder: true,
            minimumAdvanceOrderQuantity: 4,
            isAlwaysAvailable: false,
            availableDays: [DayOfWeek.Friday, DayOfWeek.Saturday],
            allergenNotes: "Puede contener trazas de frutos secos",
            priceOptions: [
                new(PortionType.Small, 3.50m),
                new(PortionType.Half, 7.00m),
                new(PortionType.Full, 14.00m)
            ]
        );

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Jamón Ibérico");
        result.Description.Should().Be("Jamón de bellota 100%");
        result.ImageUrl.Should().Be("https://example.com/jamon.jpg");
        result.DisplayOrder.Should().Be(5);
        result.IsHighRiskItem.Should().BeTrue();
        result.RequiresAdvanceOrder.Should().BeTrue();
        result.MinimumAdvanceOrderQuantity.Should().Be(4);
        result.IsAlwaysAvailable.Should().BeFalse();
        result.AvailableDays.Should().HaveCount(2);
        result.AllergenNotes.Should().Be("Puede contener trazas de frutos secos");
        result.PriceOptions.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateMenuItem_WithValidData_ShouldPersistInDatabase()
    {
        // Arrange
        var request = CreateValidRequest(name: "Tortilla Española");

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);
        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();

        var menuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(result!.Id).AsTask());

        menuItem.Should().NotBeNull();
        menuItem!.Name.Should().Be("Tortilla Española");
    }

    [Fact]
    public async Task CreateMenuItem_WithMarketPriceOption_ShouldReturnCreated()
    {
        // Arrange
        var request = CreateValidRequest(
            name: "Marisco del día",
            priceOptions: [new(PortionType.MarketPrice, null)]
        );

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.PriceOptions.Should().HaveCount(1);
        var priceOption = result.PriceOptions.First();
        priceOption.PortionType.Should().Be(PortionType.MarketPrice);
        priceOption.Price.Should().BeNull();
        priceOption.RequiresMarketPrice.Should().BeTrue();
    }

    [Fact]
    public async Task CreateMenuItem_WithMultiplePriceOptions_ShouldReturnAllOptions()
    {
        // Arrange
        var request = CreateValidRequest(
            name: "Croquetas caseras",
            priceOptions: [
                new(PortionType.Small, 4.00m),
                new(PortionType.Half, 7.50m),
                new(PortionType.Full, 12.00m)
            ]
        );

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.PriceOptions.Should().HaveCount(3);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Small && p.Price == 4.00m);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.Price == 7.50m);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full && p.Price == 12.00m);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateMenuItem_WithInvalidName_ShouldReturnUnprocessableEntity(string invalidName)
    {
        // Arrange
        var request = CreateValidRequest(name: invalidName);

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateMenuItem_WithNameExceeding100Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = CreateValidRequest(name: new string('a', 101));

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateMenuItem_WithInvalidImageUrl_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = CreateValidRequest(imageUrl: "not-a-url");

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateMenuItem_WithNoPriceOptions_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = CreateValidRequest(priceOptions: []);

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateMenuItem_WithDuplicatePortionTypes_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = CreateValidRequest(
            priceOptions: [
                new(PortionType.Full, 10.00m),
                new(PortionType.Full, 12.00m)
            ]
        );

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateMenuItem_WithRequiresAdvanceOrderWithoutHighRisk_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = CreateValidRequest(
            isHighRiskItem: false,
            requiresAdvanceOrder: true
        );

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateMenuItem_WithNotAlwaysAvailableWithoutDays_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = CreateValidRequest(
            isAlwaysAvailable: false,
            availableDays: []
        );

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateMenuItem_WithNegativePrice_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = CreateValidRequest(
            priceOptions: [new(PortionType.Full, -10.00m)]
        );

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateMenuItem_WithFixedPortionWithoutPrice_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = CreateValidRequest(
            priceOptions: [new(PortionType.Full, null)]
        );

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public async Task CreateMenuItem_WithNameExactly100Characters_ShouldReturnCreated()
    {
        // Arrange
        var name = new string('a', 100);
        var request = CreateValidRequest(name: name);

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.Name.Should().Be(name);
    }

    [Fact]
    public async Task CreateMenuItem_WithDescriptionExactly1000Characters_ShouldReturnCreated()
    {
        // Arrange
        var description = new string('a', 1000);
        var request = CreateValidRequest(description: description);

        // Act
        var response = await Client.PostAsJsonAsync("/menu-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.Description.Should().Be(description);
    }

    #endregion
}
