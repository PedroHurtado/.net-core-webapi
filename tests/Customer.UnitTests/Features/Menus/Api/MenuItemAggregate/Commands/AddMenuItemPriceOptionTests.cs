namespace Customer.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class AddMenuItemPriceOptionTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.AddPriceOption _addPriceOption;
    private readonly Mock<AddMenuItemPriceOption.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddMenuItemPriceOption.Service _service;

    public AddMenuItemPriceOptionTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _addPriceOption = new(_priceOptionCreate, _menuItemValidator);
        _repositoryMock = new Mock<AddMenuItemPriceOption.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new AddMenuItemPriceOption.Service(_addPriceOption, _repositoryMock.Object, _unitOfWorkMock.Object);
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

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_AddsPriceOption()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(portionType: PortionType.Half, price: 15.00m);

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.PriceOptions.Should().HaveCount(2);
        menuItem.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.Price == 15.00m);
    }

    [Fact]
    public async Task HandleAsync_WithSmallPortionType_AddsPriceOption()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(portionType: PortionType.Small, price: 8.00m);

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Small && p.Price == 8.00m);
    }

    [Fact]
    public async Task HandleAsync_WithMarketPriceAndNoPrice_AddsPriceOption()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(portionType: PortionType.MarketPrice, price: null);

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.PriceOptions.Should().Contain(p => p.PortionType == PortionType.MarketPrice && p.Price == null);
    }

    [Fact]
    public async Task HandleAsync_WithIsActiveFalse_AddsPriceOptionAsInactive()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(portionType: PortionType.Half, price: 15.00m, isActive: false);

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && !p.IsActive);
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
        var request = CreateValidRequest();

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.Name.Should().Be("Test MenuItem");
        menuItem.Description.Should().Be("Test Description");
        menuItem.ImageUrl.Should().Be("https://example.com/image.jpg");
        menuItem.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_PreservesExistingPriceOptions()
    {
        var menuItem = CreateMenuItem();
        var existingOption = menuItem.PriceOptions.First();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(portionType: PortionType.Half, price: 15.00m);

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.PriceOptions.Should().Contain(p =>
            p.PortionType == existingOption.PortionType &&
            p.Price == existingOption.Price);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest();

        await _service.HandleAsync(menuItem.Id, request);

        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest();

        await _service.HandleAsync(menuItem.Id, request);

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
        var request = CreateValidRequest();

        await _service.HandleAsync(menuItem.Id, request);

        callOrder.Should().ContainInOrder("Get", "SaveChanges");
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task HandleAsync_WithDuplicatePortionType_ThrowsConflictException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(portionType: PortionType.Full, price: 25.00m);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task HandleAsync_WhenConflictOccurs_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(portionType: PortionType.Full, price: 25.00m);

        try { await _service.HandleAsync(menuItem.Id, request); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_WithNegativePrice_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(portionType: PortionType.Half, price: -5.00m);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNullPriceForFixedPortionType_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(portionType: PortionType.Half, price: null);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(portionType: PortionType.Half, price: -5.00m);

        try { await _service.HandleAsync(menuItem.Id, request); } catch { }

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
        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(nonExistentId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_PropagatesException()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));
        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(id, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<AddMenuItemPriceOption.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuItemId, request)).Returns(Task.CompletedTask);

        var result = await AddMenuItemPriceOption.Handler(serviceMock.Object, menuItemId, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithIdAndRequest()
    {
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<AddMenuItemPriceOption.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<AddMenuItemPriceOption.Request>())).Returns(Task.CompletedTask);

        await AddMenuItemPriceOption.Handler(serviceMock.Object, menuItemId, request);

        serviceMock.Verify(s => s.HandleAsync(menuItemId, request), Times.Once);
    }

    #endregion
}
