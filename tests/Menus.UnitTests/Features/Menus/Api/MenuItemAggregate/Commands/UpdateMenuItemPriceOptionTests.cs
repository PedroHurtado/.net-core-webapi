namespace Menus.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class UpdateMenuItemPriceOptionTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.AddPriceOption _addPriceOption;
    private readonly MenuItemAgg.UpdatePriceOption _updatePriceOption;
    private readonly Mock<UpdateMenuItemPriceOption.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateMenuItemPriceOption.Service _service;

    public UpdateMenuItemPriceOptionTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _addPriceOption = new(_priceOptionCreate, _menuItemValidator);
        _updatePriceOption = new(_priceOptionCreate, _menuItemValidator);
        _repositoryMock = new Mock<UpdateMenuItemPriceOption.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UpdateMenuItemPriceOption.Service(_updatePriceOption, _repositoryMock.Object, _unitOfWorkMock.Object);
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

    private static UpdateMenuItemPriceOption.Request CreateValidRequest(
        decimal? price = 25.00m,
        bool isActive = true)
    {
        return new UpdateMenuItemPriceOption.Request(
            Price: price,
            IsActive: isActive
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesPriceOption()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(price: 30.00m);

        await _service.HandleAsync(menuItem.Id, PortionType.Full, request);

        menuItem.PriceOptions.First(p => p.PortionType == PortionType.Full).Price.Should().Be(30.00m);
    }

    [Fact]
    public async Task HandleAsync_DeactivatingWithOtherActiveOptions_Succeeds()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(price: 22.00m, isActive: false);

        await _service.HandleAsync(menuItem.Id, PortionType.Full, request);

        menuItem.PriceOptions.First(p => p.PortionType == PortionType.Full).IsActive.Should().BeFalse();
        menuItem.PriceOptions.First(p => p.PortionType == PortionType.Half).IsActive.Should().BeTrue();
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

        await _service.HandleAsync(menuItem.Id, PortionType.Full, request);

        menuItem.Name.Should().Be("Test MenuItem");
        menuItem.Description.Should().Be("Test Description");
        menuItem.ImageUrl.Should().Be("https://example.com/image.jpg");
        menuItem.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_PreservesOtherPriceOptions()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(price: 30.00m);

        await _service.HandleAsync(menuItem.Id, PortionType.Full, request);

        menuItem.PriceOptions.Should().HaveCount(2);
        menuItem.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.Price == 15.00m);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest();

        await _service.HandleAsync(menuItem.Id, PortionType.Full, request);

        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest();

        await _service.HandleAsync(menuItem.Id, PortionType.Full, request);

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

        await _service.HandleAsync(menuItem.Id, PortionType.Full, request);

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
        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(nonExistentId, PortionType.Full, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenPriceOptionNotFound_ThrowsKeyNotFoundException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(menuItem.Id, PortionType.Half, request);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Price option with portion type 'Half' not found*");
    }

    [Fact]
    public async Task HandleAsync_WhenPriceOptionNotFound_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest();

        try { await _service.HandleAsync(menuItem.Id, PortionType.Half, request); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_DeactivatingLastActiveOption_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(price: 22.00m, isActive: false);

        var act = () => _service.HandleAsync(menuItem.Id, PortionType.Full, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativePrice_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(price: -5.00m);

        var act = () => _service.HandleAsync(menuItem.Id, PortionType.Full, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNullPriceForFixedPortionType_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(price: null);

        var act = () => _service.HandleAsync(menuItem.Id, PortionType.Full, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(price: -5.00m);

        try { await _service.HandleAsync(menuItem.Id, PortionType.Full, request); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateMenuItemPriceOption.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuItemId, PortionType.Full, request)).Returns(Task.CompletedTask);

        var result = await UpdateMenuItemPriceOption.Handler(serviceMock.Object, menuItemId, PortionType.Full, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithIdPortionTypeAndRequest()
    {
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateMenuItemPriceOption.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<PortionType>(), It.IsAny<UpdateMenuItemPriceOption.Request>())).Returns(Task.CompletedTask);

        await UpdateMenuItemPriceOption.Handler(serviceMock.Object, menuItemId, PortionType.Full, request);

        serviceMock.Verify(s => s.HandleAsync(menuItemId, PortionType.Full, request), Times.Once);
    }

    #endregion
}
