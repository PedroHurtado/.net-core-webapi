namespace Customer.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class ActivateMenuItemTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.Activate _activateMenuItem;
    private readonly Mock<ActivateMenuItem.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ActivateMenuItem.Service _service;

    public ActivateMenuItemTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _activateMenuItem = new(_menuItemValidator);
        _repositoryMock = new Mock<ActivateMenuItem.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new ActivateMenuItem.Service(_activateMenuItem, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private TestableMenuItem CreateInactiveMenuItem(string name = "Inactive MenuItem")
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = _tenantId,
            Name = name,
            IsActive = false,
            IsAvailable = true,
            IsAlwaysAvailable = true
        };

        var priceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 22.00m, IsActive: true));
        menuItem.AddPriceOptionDirect(priceOption);

        return menuItem;
    }

    private TestableMenuItem CreateActiveMenuItem(string name = "Active MenuItem")
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = _tenantId,
            Name = name,
            IsActive = true,
            IsAvailable = true,
            IsAlwaysAvailable = true
        };

        var priceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 22.00m, IsActive: true));
        menuItem.AddPriceOptionDirect(priceOption);

        return menuItem;
    }

    private TestableMenuItem CreateInactiveMenuItemWithNoActivePriceOptions(string name = "No Active Prices")
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = _tenantId,
            Name = name,
            IsActive = false,
            IsAvailable = true,
            IsAlwaysAvailable = true
        };

        var priceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 22.00m, IsActive: false));
        menuItem.AddPriceOptionDirect(priceOption);

        return menuItem;
    }

    private void SetupRepositoryGet(MenuItemAgg menuItem)
    {
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithInactiveMenuItem_ActivatesMenuItem()
    {
        var menuItem = CreateInactiveMenuItem(name: "Pulpo al Horno");
        SetupRepositoryGet(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Should().NotBeNull();
        response.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithInactiveMenuItem_ReturnsCorrectResponse()
    {
        var menuItem = CreateInactiveMenuItem(name: "Jamón Ibérico");
        menuItem.Description = "Jamón de bellota 100%";
        SetupRepositoryGet(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Id.Should().Be(menuItem.Id);
        response.TenantId.Should().Be(_tenantId);
        response.Name.Should().Be("Jamón Ibérico");
        response.Description.Should().Be("Jamón de bellota 100%");
        response.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_PreservesOtherProperties()
    {
        var menuItem = CreateInactiveMenuItem();
        menuItem.Description = "Test Description";
        menuItem.ImageUrl = "https://example.com/image.jpg";
        menuItem.DisplayOrder = 5;
        SetupRepositoryGet(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Description.Should().Be("Test Description");
        response.ImageUrl.Should().Be("https://example.com/image.jpg");
        response.DisplayOrder.Should().Be(5);
        response.IsAvailable.Should().BeTrue();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuItem = CreateInactiveMenuItem();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuItem = CreateInactiveMenuItem();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetsMenuItemBeforeSaving()
    {
        var menuItem = CreateInactiveMenuItem();
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

    #region Conflict Tests

    [Fact]
    public async Task HandleAsync_WhenMenuItemAlreadyActive_ThrowsConflictException()
    {
        var menuItem = CreateActiveMenuItem();
        SetupRepositoryGet(menuItem);

        var act = () => _service.HandleAsync(menuItem.Id);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Menu item is already active");
    }

    [Fact]
    public async Task HandleAsync_WhenConflict_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateActiveMenuItem();
        SetupRepositoryGet(menuItem);

        try { await _service.HandleAsync(menuItem.Id); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_WhenNoActivePriceOptions_ThrowsValidationException()
    {
        var menuItem = CreateInactiveMenuItemWithNoActivePriceOptions();
        SetupRepositoryGet(menuItem);

        var act = () => _service.HandleAsync(menuItem.Id);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateInactiveMenuItemWithNoActivePriceOptions();
        SetupRepositoryGet(menuItem);

        try { await _service.HandleAsync(menuItem.Id); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
        var serviceMock = new Mock<ActivateMenuItem.IService>();
        var expectedResponse = new MenuItemResponse(
            Id: menuItemId,
            TenantId: _tenantId,
            Name: "Test Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsActive: true,
            IsAvailable: true,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AllergenNotes: null,
            IsAvailableToday: true,
            CanBeOrdered: true,
            HasDepositOverride: false,
            DepositOverride: null,
            NutritionalInfo: null,
            PriceOptions: [],
            AvailableDays: [],
            Allergens: []
        );

        serviceMock.Setup(s => s.HandleAsync(menuItemId)).ReturnsAsync(expectedResponse);

        var result = await ActivateMenuItem.Handler(serviceMock.Object, menuItemId);

        result.Should().BeOfType<Ok<MenuItemResponse>>();
        var okResult = (Ok<MenuItemResponse>)result;
        okResult.Value.Should().Be(expectedResponse);
        okResult.Value!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_CallsServiceWithId()
    {
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<ActivateMenuItem.IService>();
        var expectedResponse = new MenuItemResponse(
            Id: menuItemId,
            TenantId: _tenantId,
            Name: "Test",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsActive: true,
            IsAvailable: true,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AllergenNotes: null,
            IsAvailableToday: true,
            CanBeOrdered: true,
            HasDepositOverride: false,
            DepositOverride: null,
            NutritionalInfo: null,
            PriceOptions: [],
            AvailableDays: [],
            Allergens: []
        );

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>())).ReturnsAsync(expectedResponse);

        await ActivateMenuItem.Handler(serviceMock.Object, menuItemId);

        serviceMock.Verify(s => s.HandleAsync(menuItemId), Times.Once);
    }

    #endregion
}
