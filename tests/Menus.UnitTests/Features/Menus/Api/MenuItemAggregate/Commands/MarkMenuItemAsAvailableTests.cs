namespace Menus.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class MarkMenuItemAsAvailableTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.MarkAsAvailable _markAsAvailable;
    private readonly Mock<MarkMenuItemAsAvailable.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MarkMenuItemAsAvailable.Service _service;

    public MarkMenuItemAsAvailableTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _markAsAvailable = new(_menuItemValidator);
        _repositoryMock = new Mock<MarkMenuItemAsAvailable.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new MarkMenuItemAsAvailable.Service(_markAsAvailable, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static CreatePriceOptionCommand[] CreateValidPriceOptionCommands() =>
    [
        new CreatePriceOptionCommand(PortionType.Full, 22.00m)
    ];

    private MenuItemAgg CreateMenuItem(string name = "Test MenuItem")
    {
        var command = new CreateMenuItemCommand(
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
        );

        return _createMenuItem.Execute(command);
    }

    private void SetupRepositoryGet(MenuItemAgg menuItem)
    {
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithMenuItem_ReturnsAvailableResponse()
    {
        var menuItem = CreateMenuItem(name: "Pulpo al Horno");
        SetupRepositoryGet(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Should().NotBeNull();
        response.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithMenuItem_ReturnsCorrectResponse()
    {
        var createCommand = new CreateMenuItemCommand(
            TenantId: _tenantId,
            Name: "Jamón Ibérico",
            Description: "Jamón de bellota 100%",
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: CreateValidPriceOptionCommands()
        );
        var menuItem = _createMenuItem.Execute(createCommand);
        SetupRepositoryGet(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Id.Should().Be(menuItem.Id);
        response.Name.Should().Be("Jamón Ibérico");
        response.Description.Should().Be("Jamón de bellota 100%");
        response.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_PreservesOtherProperties()
    {
        var createCommand = new CreateMenuItemCommand(
            TenantId: _tenantId,
            Name: "Test MenuItem",
            Description: "Test Description",
            ImageUrl: "https://example.com/image.jpg",
            DisplayOrder: 5,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: CreateValidPriceOptionCommands()
        );
        var menuItem = _createMenuItem.Execute(createCommand);
        SetupRepositoryGet(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Description.Should().Be("Test Description");
        response.ImageUrl.Should().Be("https://example.com/image.jpg");
        response.DisplayOrder.Should().Be(5);
        response.IsActive.Should().BeFalse();
    }

    #endregion

    #region Idempotent Tests

    [Fact]
    public async Task HandleAsync_WithAvailableMenuItem_RemainsAvailable()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Should().NotBeNull();
        response.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithAvailableMenuItem_DoesNotThrow()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        var act = () => _service.HandleAsync(menuItem.Id);

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetsMenuItemBeforeSaving()
    {
        var menuItem = CreateMenuItem();
        var callOrder = new List<string>();
        _repositoryMock.Setup(r => r.Get(menuItem.Id))
            .Callback(() => callOrder.Add("Get"))
            .ReturnsAsync(menuItem);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        await _service.HandleAsync(menuItem.Id);

        callOrder.Should().ContainInOrder("Get", "SaveChanges");
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
        var serviceMock = new Mock<MarkMenuItemAsAvailable.IService>();
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

        var result = await MarkMenuItemAsAvailable.Handler(serviceMock.Object, menuItemId);

        result.Should().BeOfType<Ok<MenuItemResponse>>();
        var okResult = (Ok<MenuItemResponse>)result;
        okResult.Value.Should().Be(expectedResponse);
        okResult.Value!.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_CallsServiceWithId()
    {
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<MarkMenuItemAsAvailable.IService>();
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

        await MarkMenuItemAsAvailable.Handler(serviceMock.Object, menuItemId);

        serviceMock.Verify(s => s.HandleAsync(menuItemId), Times.Once);
    }

    #endregion
}
