namespace Customer.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class DeactivateMenuItemTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.Deactivate _deactivateMenuItem;
    private readonly Mock<DeactivateMenuItem.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateMenuItem.Service _service;

    public DeactivateMenuItemTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _deactivateMenuItem = new(_menuItemValidator);
        _repositoryMock = new Mock<DeactivateMenuItem.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new DeactivateMenuItem.Service(_deactivateMenuItem, _repositoryMock.Object, _unitOfWorkMock.Object);
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

        var priceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 22.00m));
        menuItem.AddPriceOptionDirect(priceOption);

        return menuItem;
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

        var priceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 22.00m));
        menuItem.AddPriceOptionDirect(priceOption);

        return menuItem;
    }

    private void SetupRepositoryGet(MenuItemAgg menuItem)
    {
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithActiveMenuItem_DeactivatesMenuItem()
    {
        var menuItem = CreateActiveMenuItem(name: "Pulpo al Horno");
        SetupRepositoryGet(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Should().NotBeNull();
        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithActiveMenuItem_ReturnsCorrectResponse()
    {
        var menuItem = CreateActiveMenuItem(name: "Jamón Ibérico");
        menuItem.Description = "Jamón de bellota 100%";
        SetupRepositoryGet(menuItem);

        var response = await _service.HandleAsync(menuItem.Id);

        response.Id.Should().Be(menuItem.Id);
        response.TenantId.Should().Be(_tenantId);
        response.Name.Should().Be("Jamón Ibérico");
        response.Description.Should().Be("Jamón de bellota 100%");
        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_PreservesOtherProperties()
    {
        var menuItem = CreateActiveMenuItem();
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
        var menuItem = CreateActiveMenuItem();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuItem = CreateActiveMenuItem();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetsMenuItemBeforeSaving()
    {
        var menuItem = CreateActiveMenuItem();
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
    public async Task HandleAsync_WhenMenuItemAlreadyInactive_ThrowsConflictException()
    {
        var menuItem = CreateInactiveMenuItem();
        SetupRepositoryGet(menuItem);

        var act = () => _service.HandleAsync(menuItem.Id);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Menu item is already inactive");
    }

    [Fact]
    public async Task HandleAsync_WhenConflict_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateInactiveMenuItem();
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
        var serviceMock = new Mock<DeactivateMenuItem.IService>();
        var expectedResponse = new MenuItemResponse(
            Id: menuItemId,
            TenantId: _tenantId,
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

        var result = await DeactivateMenuItem.Handler(serviceMock.Object, menuItemId);

        result.Should().BeOfType<Ok<MenuItemResponse>>();
        var okResult = (Ok<MenuItemResponse>)result;
        okResult.Value.Should().Be(expectedResponse);
        okResult.Value!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handler_CallsServiceWithId()
    {
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<DeactivateMenuItem.IService>();
        var expectedResponse = new MenuItemResponse(
            Id: menuItemId,
            TenantId: _tenantId,
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

        await DeactivateMenuItem.Handler(serviceMock.Object, menuItemId);

        serviceMock.Verify(s => s.HandleAsync(menuItemId), Times.Once);
    }

    #endregion
}
