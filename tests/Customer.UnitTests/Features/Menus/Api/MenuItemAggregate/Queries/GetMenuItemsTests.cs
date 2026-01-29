namespace Customer.UnitTests.Features.Menus.Api.MenuItemAggregate.Queries;

public class GetMenuItemsTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly Mock<IQuery> _queryMock;
    private readonly GetMenuItems.Service _service;

    public GetMenuItemsTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _queryMock = new Mock<IQuery>();
        _service = new GetMenuItems.Service(_queryMock.Object);
    }

    private static CreatePriceOptionCommand[] CreateValidPriceOptionCommands() =>
    [
        new CreatePriceOptionCommand(PortionType.Full, 22.00m)
    ];

    private MenuItemAgg CreateMenuItem(
        string name = "Test MenuItem",
        bool isActive = false,
        bool isAvailable = true)
    {
        var menuItem = _createMenuItem.Execute(new CreateMenuItemCommand(
            TenantId: _tenantId,
            Name: name,
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: CreateValidPriceOptionCommands()
        ));

        if (isActive)
        {
            menuItem.GetType().GetProperty("IsActive")!.SetValue(menuItem, true);
        }

        if (!isAvailable)
        {
            menuItem.GetType().GetProperty("IsAvailable")!.SetValue(menuItem, false);
        }

        return menuItem;
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithMenuItems_ReturnsAllMenuItems()
    {
        // Arrange
        var menuItems = new List<MenuItemAgg>
        {
            CreateMenuItem("Tortilla Española"),
            CreateMenuItem("Paella Mixta")
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuItemAgg>()).Returns(menuItems);

        // Act
        var response = await _service.HandleAsync(null, null);

        // Assert
        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WithNoMenuItems_ReturnsEmptyList()
    {
        // Arrange
        var menuItems = new List<MenuItemAgg>().AsAsyncQueryable();
        _queryMock.Setup(q => q.Query<MenuItemAgg>()).Returns(menuItems);

        // Act
        var response = await _service.HandleAsync(null, null);

        // Assert
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsCorrectlyMappedResponse()
    {
        // Arrange
        var menuItem = CreateMenuItem("Gambas al Ajillo");
        var menuItems = new List<MenuItemAgg> { menuItem }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuItemAgg>()).Returns(menuItems);

        // Act
        var response = await _service.HandleAsync(null, null);

        // Assert
        response.Should().HaveCount(1);
        var result = response.First();
        result.Id.Should().Be(menuItem.Id);
        result.Name.Should().Be("Gambas al Ajillo");
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task HandleAsync_WithIsActiveTrue_ReturnsOnlyActiveMenuItems()
    {
        // Arrange
        var menuItems = new List<MenuItemAgg>
        {
            CreateMenuItem("MenuItem Activo", isActive: true),
            CreateMenuItem("MenuItem Inactivo", isActive: false)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuItemAgg>()).Returns(menuItems);

        // Act
        var response = await _service.HandleAsync(isActive: true, isAvailable: null);

        // Assert
        response.Should().HaveCount(1);
        response.First().Name.Should().Be("MenuItem Activo");
        response.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithIsActiveFalse_ReturnsOnlyInactiveMenuItems()
    {
        // Arrange
        var menuItems = new List<MenuItemAgg>
        {
            CreateMenuItem("MenuItem Activo", isActive: true),
            CreateMenuItem("MenuItem Inactivo", isActive: false)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuItemAgg>()).Returns(menuItems);

        // Act
        var response = await _service.HandleAsync(isActive: false, isAvailable: null);

        // Assert
        response.Should().HaveCount(1);
        response.First().Name.Should().Be("MenuItem Inactivo");
        response.First().IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithIsAvailableTrue_ReturnsOnlyAvailableMenuItems()
    {
        // Arrange
        var menuItems = new List<MenuItemAgg>
        {
            CreateMenuItem("MenuItem Disponible", isAvailable: true),
            CreateMenuItem("MenuItem No Disponible", isAvailable: false)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuItemAgg>()).Returns(menuItems);

        // Act
        var response = await _service.HandleAsync(isActive: null, isAvailable: true);

        // Assert
        response.Should().HaveCount(1);
        response.First().Name.Should().Be("MenuItem Disponible");
        response.First().IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithIsAvailableFalse_ReturnsOnlyUnavailableMenuItems()
    {
        // Arrange
        var menuItems = new List<MenuItemAgg>
        {
            CreateMenuItem("MenuItem Disponible", isAvailable: true),
            CreateMenuItem("MenuItem No Disponible", isAvailable: false)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuItemAgg>()).Returns(menuItems);

        // Act
        var response = await _service.HandleAsync(isActive: null, isAvailable: false);

        // Assert
        response.Should().HaveCount(1);
        response.First().Name.Should().Be("MenuItem No Disponible");
        response.First().IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithBothFilters_ReturnsFilteredMenuItems()
    {
        // Arrange
        var menuItems = new List<MenuItemAgg>
        {
            CreateMenuItem("Activo Disponible", isActive: true, isAvailable: true),
            CreateMenuItem("Activo No Disponible", isActive: true, isAvailable: false),
            CreateMenuItem("Inactivo Disponible", isActive: false, isAvailable: true),
            CreateMenuItem("Inactivo No Disponible", isActive: false, isAvailable: false)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<MenuItemAgg>()).Returns(menuItems);

        // Act
        var response = await _service.HandleAsync(isActive: true, isAvailable: true);

        // Assert
        response.Should().HaveCount(1);
        response.First().Name.Should().Be("Activo Disponible");
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryQuery()
    {
        // Arrange
        var menuItems = new List<MenuItemAgg>().AsAsyncQueryable();
        _queryMock.Setup(q => q.Query<MenuItemAgg>()).Returns(menuItems);

        // Act
        await _service.HandleAsync(null, null);

        // Assert
        _queryMock.Verify(q => q.Query<MenuItemAgg>(), Times.Once);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_ReturnsOkResult()
    {
        // Arrange
        var serviceMock = new Mock<GetMenuItems.IService>();
        var expectedResponse = new List<MenuItemResponse>
        {
            new(Guid.NewGuid(), _tenantId, "Test", null, null, 0, true, true, false, false, null,
                true, null, true, true, false, null, null, [], [], [])
        };

        serviceMock.Setup(s => s.HandleAsync(null, null)).ReturnsAsync(expectedResponse);

        // Act
        var result = await GetMenuItems.Handler(serviceMock.Object, null, null);

        // Assert
        result.Should().BeOfType<Ok<List<MenuItemResponse>>>();
        var okResult = (Ok<List<MenuItemResponse>>)result;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithFilters()
    {
        // Arrange
        var serviceMock = new Mock<GetMenuItems.IService>();
        serviceMock.Setup(s => s.HandleAsync(true, true)).ReturnsAsync([]);

        // Act
        await GetMenuItems.Handler(serviceMock.Object, true, true);

        // Assert
        serviceMock.Verify(s => s.HandleAsync(true, true), Times.Once);
    }

    #endregion
}
