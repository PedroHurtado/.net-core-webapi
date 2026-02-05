namespace Menus.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemPriceOptionTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.AddPriceOption _addPriceOption;
    private readonly MenuItemAgg.RemovePriceOption _removePriceOption;
    private readonly Mock<RemoveMenuItemPriceOption.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveMenuItemPriceOption.Service _service;

    public RemoveMenuItemPriceOptionTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _addPriceOption = new(_priceOptionCreate, _menuItemValidator);
        _removePriceOption = new(_menuItemValidator);
        _repositoryMock = new Mock<RemoveMenuItemPriceOption.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new RemoveMenuItemPriceOption.Service(_removePriceOption, _repositoryMock.Object, _unitOfWorkMock.Object);
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

    private MenuItemAgg CreateMenuItemWithMultiplePriceOptions()
    {
        var menuItem = CreateMenuItem();
        _addPriceOption.Execute(menuItem, new AddPriceOptionCommand(PortionType.Half, 15.00m));
        return menuItem;
    }

    private void SetupRepositoryGet(MenuItemAgg menuItem)
    {
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidPriceOption_RemovesPriceOption()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id, PortionType.Half);

        menuItem.PriceOptions.Should().NotContain(p => p.PortionType == PortionType.Half);
    }

    [Fact]
    public async Task HandleAsync_WithValidPriceOption_PreservesOtherPriceOptions()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id, PortionType.Half);

        menuItem.PriceOptions.Should().HaveCount(1);
        menuItem.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full && p.Price == 22.00m);
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
        _addPriceOption.Execute(menuItem, new AddPriceOptionCommand(PortionType.Half, 15.00m));
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id, PortionType.Half);

        menuItem.Name.Should().Be("Test MenuItem");
        menuItem.Description.Should().Be("Test Description");
        menuItem.ImageUrl.Should().Be("https://example.com/image.jpg");
        menuItem.DisplayOrder.Should().Be(5);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id, PortionType.Half);

        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id, PortionType.Half);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetsMenuItemBeforeSaving()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        var callOrder = new List<string>();
        _repositoryMock.Setup(r => r.Get(menuItem.Id))
            .Callback(() => callOrder.Add("Get"))
            .ReturnsAsync(menuItem);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        await _service.HandleAsync(menuItem.Id, PortionType.Half);

        callOrder.Should().ContainInOrder("Get", "SaveChanges");
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task HandleAsync_WhenMenuItemNotFound_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException("MenuItem not found"));

        var act = () => _service.HandleAsync(nonExistentId, PortionType.Full);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenPriceOptionNotFound_ThrowsKeyNotFoundException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        var act = () => _service.HandleAsync(menuItem.Id, PortionType.Half);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Price option with portion type 'Half' not found*");
    }

    [Fact]
    public async Task HandleAsync_WhenPriceOptionNotFound_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        try { await _service.HandleAsync(menuItem.Id, PortionType.Half); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_RemovingLastPriceOption_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        var act = () => _service.HandleAsync(menuItem.Id, PortionType.Full);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        try { await _service.HandleAsync(menuItem.Id, PortionType.Full); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveMenuItemPriceOption.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuItemId, PortionType.Half)).Returns(Task.CompletedTask);

        var result = await RemoveMenuItemPriceOption.Handler(serviceMock.Object, menuItemId, PortionType.Half);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithIdAndPortionType()
    {
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveMenuItemPriceOption.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<PortionType>())).Returns(Task.CompletedTask);

        await RemoveMenuItemPriceOption.Handler(serviceMock.Object, menuItemId, PortionType.Half);

        serviceMock.Verify(s => s.HandleAsync(menuItemId, PortionType.Half), Times.Once);
    }

    #endregion
}
