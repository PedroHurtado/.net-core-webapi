namespace Menus.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class SetMenuItemDepositOverrideTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly ItemDepositOverrideValidator _depositOverrideValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly ItemDepositOverrideVO.Create _depositOverrideCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.SetDepositOverride _setDepositOverride;
    private readonly Mock<SetMenuItemDepositOverride.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SetMenuItemDepositOverride.Service _service;

    public SetMenuItemDepositOverrideTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _depositOverrideCreate = new(_depositOverrideValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _setDepositOverride = new(_depositOverrideCreate, _menuItemValidator);
        _repositoryMock = new Mock<SetMenuItemDepositOverride.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new SetMenuItemDepositOverride.Service(_setDepositOverride, _repositoryMock.Object, _unitOfWorkMock.Object);
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

    private static SetMenuItemDepositOverride.Request CreateValidRequest(
        decimal depositAmount = 30.00m,
        int? minimumQuantityForDeposit = null)
    {
        return new SetMenuItemDepositOverride.Request(
            DepositAmount: depositAmount,
            MinimumQuantityForDeposit: minimumQuantityForDeposit
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_SetsDepositOverride()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(depositAmount: 30.00m);

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.HasDepositOverride.Should().BeTrue();
        menuItem.DepositOverride!.DepositAmount.Should().Be(30.00m);
    }

    [Fact]
    public async Task HandleAsync_WithoutMinimumQuantity_SetsAppliesToAllQuantities()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(depositAmount: 30.00m, minimumQuantityForDeposit: null);

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.DepositOverride!.AppliesToAllQuantities.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithMinimumQuantity_SetsMinimumQuantityForDeposit()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(depositAmount: 30.00m, minimumQuantityForDeposit: 4);

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.DepositOverride!.MinimumQuantityForDeposit.Should().Be(4);
        menuItem.DepositOverride.AppliesToAllQuantities.Should().BeFalse();
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
        var request = CreateValidRequest(depositAmount: 30.00m);

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.Name.Should().Be("Test MenuItem");
        menuItem.Description.Should().Be("Test Description");
        menuItem.ImageUrl.Should().Be("https://example.com/image.jpg");
        menuItem.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_ReplacesExistingDepositOverride()
    {
        var menuItem = CreateMenuItem();
        _setDepositOverride.Execute(menuItem, new SetDepositOverrideCommand(20.00m, 2));
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(depositAmount: 50.00m, minimumQuantityForDeposit: 5);

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.DepositOverride!.DepositAmount.Should().Be(50.00m);
        menuItem.DepositOverride.MinimumQuantityForDeposit.Should().Be(5);
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

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_WithZeroDepositAmount_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(depositAmount: 0m);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativeDepositAmount_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(depositAmount: -10.00m);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithZeroMinimumQuantity_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(depositAmount: 30.00m, minimumQuantityForDeposit: 0);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(depositAmount: 0m);

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
        var serviceMock = new Mock<SetMenuItemDepositOverride.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuItemId, request)).Returns(Task.CompletedTask);

        var result = await SetMenuItemDepositOverride.Handler(serviceMock.Object, menuItemId, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithIdAndRequest()
    {
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<SetMenuItemDepositOverride.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<SetMenuItemDepositOverride.Request>())).Returns(Task.CompletedTask);

        await SetMenuItemDepositOverride.Handler(serviceMock.Object, menuItemId, request);

        serviceMock.Verify(s => s.HandleAsync(menuItemId, request), Times.Once);
    }

    #endregion
}
