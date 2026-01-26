namespace Customer.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemDepositOverrideTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly ItemDepositOverrideValidator _depositOverrideValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly ItemDepositOverrideVO.Create _depositOverrideCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.SetDepositOverride _setDepositOverride;
    private readonly MenuItemAgg.RemoveDepositOverride _removeDepositOverride;
    private readonly Mock<RemoveMenuItemDepositOverride.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveMenuItemDepositOverride.Service _service;

    public RemoveMenuItemDepositOverrideTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _depositOverrideCreate = new(_depositOverrideValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _setDepositOverride = new(_depositOverrideCreate, _menuItemValidator);
        _removeDepositOverride = new(_menuItemValidator);
        _repositoryMock = new Mock<RemoveMenuItemDepositOverride.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new RemoveMenuItemDepositOverride.Service(_removeDepositOverride, _repositoryMock.Object, _unitOfWorkMock.Object);
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

    private MenuItemAgg CreateMenuItemWithDepositOverride(
        string name = "Test MenuItem",
        decimal depositAmount = 30.00m,
        int? minimumQuantity = null)
    {
        var menuItem = CreateMenuItem(name);
        return _setDepositOverride.Execute(menuItem, new SetDepositOverrideCommand(depositAmount, minimumQuantity));
    }

    private void SetupRepositoryGet(MenuItemAgg menuItem)
    {
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithExistingDepositOverride_RemovesDepositOverride()
    {
        var menuItem = CreateMenuItemWithDepositOverride();
        SetupRepositoryGet(menuItem);
        menuItem.HasDepositOverride.Should().BeTrue();

        await _service.HandleAsync(menuItem.Id);

        menuItem.HasDepositOverride.Should().BeFalse();
        menuItem.DepositOverride.Should().BeNull();
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
        _setDepositOverride.Execute(menuItem, new SetDepositOverrideCommand(30.00m));
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        menuItem.Name.Should().Be("Test MenuItem");
        menuItem.Description.Should().Be("Test Description");
        menuItem.ImageUrl.Should().Be("https://example.com/image.jpg");
        menuItem.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_PreservesPriceOptions()
    {
        var menuItem = CreateMenuItemWithDepositOverride();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        menuItem.PriceOptions.Should().HaveCount(1);
    }

    #endregion

    #region Idempotent Tests

    [Fact]
    public async Task HandleAsync_WithoutDepositOverride_DoesNotThrow()
    {
        var menuItem = CreateMenuItem();
        menuItem.HasDepositOverride.Should().BeFalse();
        SetupRepositoryGet(menuItem);

        var act = () => _service.HandleAsync(menuItem.Id);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_WithoutDepositOverride_RemainsWithoutOverride()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        menuItem.HasDepositOverride.Should().BeFalse();
        menuItem.DepositOverride.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_CalledTwice_ProducesSameResult()
    {
        var menuItem = CreateMenuItemWithDepositOverride();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);
        await _service.HandleAsync(menuItem.Id);

        menuItem.HasDepositOverride.Should().BeFalse();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuItem = CreateMenuItemWithDepositOverride();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuItem = CreateMenuItemWithDepositOverride();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetsMenuItemBeforeSaving()
    {
        var menuItem = CreateMenuItemWithDepositOverride();
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
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveMenuItemDepositOverride.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuItemId)).Returns(Task.CompletedTask);

        var result = await RemoveMenuItemDepositOverride.Handler(serviceMock.Object, menuItemId);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithId()
    {
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveMenuItemDepositOverride.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await RemoveMenuItemDepositOverride.Handler(serviceMock.Object, menuItemId);

        serviceMock.Verify(s => s.HandleAsync(menuItemId), Times.Once);
    }

    #endregion
}
