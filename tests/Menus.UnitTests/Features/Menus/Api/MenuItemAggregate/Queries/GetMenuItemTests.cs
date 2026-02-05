namespace Menus.UnitTests.Features.Menus.Api.MenuItemAggregate.Queries;

public class GetMenuItemTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly Mock<GetMenuItem.IRepository> _repositoryMock;
    private readonly GetMenuItem.Service _service;

    public GetMenuItemTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _repositoryMock = new Mock<GetMenuItem.IRepository>();
        _service = new GetMenuItem.Service(_repositoryMock.Object);
    }

    private MenuItemAgg CreateMenuItem(
        string name = "Test MenuItem",
        string? description = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isHighRiskItem = false,
        bool requiresAdvanceOrder = false,
        int? minimumAdvanceOrderQuantity = null,
        bool isAlwaysAvailable = true,
        DayOfWeek[]? availableDays = null,
        string? allergenNotes = null,
        CreatePriceOptionCommand[]? priceOptions = null)
    {
        return _createMenuItem.Execute(new CreateMenuItemCommand(
            TenantId: _tenantId,
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
            PriceOptions: priceOptions ?? [new CreatePriceOptionCommand(PortionType.Full, 22.00m)]
        ));
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithExistingId_ReturnsResponse()
    {
        var menuItem = CreateMenuItem(name: "Pulpo al Horno");
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Should().NotBeNull();
        response.Id.Should().Be(menuItem.Id);
        response.Name.Should().Be("Pulpo al Horno");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_ReturnsCompleteResponse()
    {
        var menuItem = CreateMenuItem(
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
                new CreatePriceOptionCommand(PortionType.Small, 3.50m),
                new CreatePriceOptionCommand(PortionType.Half, 7.00m),
                new CreatePriceOptionCommand(PortionType.Full, 14.00m)
            ]
        );
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Id.Should().Be(menuItem.Id);
        response.Name.Should().Be("Jamón Ibérico");
        response.Description.Should().Be("Jamón de bellota 100%");
        response.ImageUrl.Should().Be("https://example.com/jamon.jpg");
        response.DisplayOrder.Should().Be(5);
        response.IsHighRiskItem.Should().BeTrue();
        response.RequiresAdvanceOrder.Should().BeTrue();
        response.MinimumAdvanceOrderQuantity.Should().Be(4);
        response.IsAlwaysAvailable.Should().BeFalse();
        response.AvailableDays.Should().HaveCount(2);
        response.AllergenNotes.Should().Be("Puede contener trazas");
        response.PriceOptions.Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_WithNullOptionalFields_ReturnsNullValues()
    {
        var menuItem = CreateMenuItem(
            name: "Simple Item",
            description: null,
            imageUrl: null,
            allergenNotes: null
        );
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Description.Should().BeNull();
        response.ImageUrl.Should().BeNull();
        response.AllergenNotes.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithDefaultValues_ReturnsDefaults()
    {
        var menuItem = CreateMenuItem();
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.IsActive.Should().BeFalse();
        response.IsAvailable.Should().BeTrue();
        response.IsAlwaysAvailable.Should().BeTrue();
        response.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_MapsComputedProperties()
    {
        var menuItem = CreateMenuItem(isAlwaysAvailable: true);
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.IsAvailableToday.Should().BeTrue();
        response.CanBeOrdered.Should().BeFalse();
        response.HasDepositOverride.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_MapsPriceOptionsCorrectly()
    {
        var menuItem = CreateMenuItem(
            priceOptions: [
                new CreatePriceOptionCommand(PortionType.Full, 14.00m),
                new CreatePriceOptionCommand(PortionType.MarketPrice, null)
            ]
        );
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.PriceOptions.Should().HaveCount(2);
        response.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full && p.Price == 14.00m);
        response.PriceOptions.Should().Contain(p => p.PortionType == PortionType.MarketPrice && p.RequiresMarketPrice);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuItem = CreateMenuItem();
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);

        await _service.HandleAsync(menuItem.Id);

        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PassesCorrectIdToRepository()
    {
        var menuItem = CreateMenuItem();
        var specificId = menuItem.Id;
        _repositoryMock.Setup(r => r.Get(specificId)).ReturnsAsync(menuItem);

        await _service.HandleAsync(specificId);

        _repositoryMock.Verify(r => r.Get(specificId), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task HandleAsync_WhenMenuItemNotFound_ThrowsException()
    {
        var nonExistentId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException("MenuItem not found"));

        var act = () => _service.HandleAsync(nonExistentId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_PropagatesException()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var act = () => _service.HandleAsync(id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidId_ReturnsOkResult()
    {
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<GetMenuItem.IService>();
        var expectedResponse = new MenuItemResponse(
            Id: menuItemId,
            Name: "Test Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsActive: false,
            IsAvailable: true,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AllergenNotes: null,
            IsAvailableToday: true,
            CanBeOrdered: false,
            HasDepositOverride: false,
            DepositOverride: null,
            NutritionalInfo: null,
            PriceOptions: [],
            AvailableDays: [],
            Allergens: []
        );

        serviceMock.Setup(s => s.HandleAsync(menuItemId)).ReturnsAsync(expectedResponse);

        var result = await GetMenuItem.Handler(serviceMock.Object, menuItemId);

        result.Should().BeOfType<Ok<MenuItemResponse>>();
        var okResult = (Ok<MenuItemResponse>)result;
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithId()
    {
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<GetMenuItem.IService>();
        var expectedResponse = new MenuItemResponse(
            Id: menuItemId,
            Name: "Test",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsActive: false,
            IsAvailable: true,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AllergenNotes: null,
            IsAvailableToday: true,
            CanBeOrdered: false,
            HasDepositOverride: false,
            DepositOverride: null,
            NutritionalInfo: null,
            PriceOptions: [],
            AvailableDays: [],
            Allergens: []
        );

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>())).ReturnsAsync(expectedResponse);

        await GetMenuItem.Handler(serviceMock.Object, menuItemId);

        serviceMock.Verify(s => s.HandleAsync(menuItemId), Times.Once);
    }

    #endregion
}
