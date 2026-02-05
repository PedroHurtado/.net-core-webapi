namespace Menus.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemNutritionalInfoTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly NutritionalInfoValidator _nutritionalInfoValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly NutritionalInfoVO.Create _nutritionalInfoCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.SetNutritionalInfo _setNutritionalInfo;
    private readonly MenuItemAgg.RemoveNutritionalInfo _removeNutritionalInfo;
    private readonly Mock<RemoveMenuItemNutritionalInfo.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveMenuItemNutritionalInfo.Service _service;

    public RemoveMenuItemNutritionalInfoTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _nutritionalInfoCreate = new(_nutritionalInfoValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _setNutritionalInfo = new(_nutritionalInfoCreate, _menuItemValidator);
        _removeNutritionalInfo = new(_menuItemValidator);
        _repositoryMock = new Mock<RemoveMenuItemNutritionalInfo.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new RemoveMenuItemNutritionalInfo.Service(_removeNutritionalInfo, _repositoryMock.Object, _unitOfWorkMock.Object);
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

    private MenuItemAgg CreateMenuItemWithNutritionalInfo(
        string name = "Test MenuItem",
        int calories = 500,
        decimal protein = 25.0m,
        decimal carbohydrates = 40.0m,
        decimal fat = 15.0m,
        int servingSize = 350)
    {
        var menuItem = CreateMenuItem(name);
        return _setNutritionalInfo.Execute(menuItem, new SetNutritionalInfoCommand(
            calories, protein, carbohydrates, fat, servingSize));
    }

    private void SetupRepositoryGet(MenuItemAgg menuItem)
    {
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithExistingNutritionalInfo_RemovesNutritionalInfo()
    {
        var menuItem = CreateMenuItemWithNutritionalInfo();
        SetupRepositoryGet(menuItem);
        menuItem.NutritionalInfo.Should().NotBeNull();

        await _service.HandleAsync(menuItem.Id);

        menuItem.NutritionalInfo.Should().BeNull();
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
        _setNutritionalInfo.Execute(menuItem, new SetNutritionalInfoCommand(500, 25.0m, 40.0m, 15.0m, 350));
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
        var menuItem = CreateMenuItemWithNutritionalInfo();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        menuItem.PriceOptions.Should().HaveCount(1);
    }

    #endregion

    #region Idempotent Tests

    [Fact]
    public async Task HandleAsync_WithoutNutritionalInfo_DoesNotThrow()
    {
        var menuItem = CreateMenuItem();
        menuItem.NutritionalInfo.Should().BeNull();
        SetupRepositoryGet(menuItem);

        var act = () => _service.HandleAsync(menuItem.Id);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_WithoutNutritionalInfo_RemainsWithoutNutritionalInfo()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        menuItem.NutritionalInfo.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_CalledTwice_ProducesSameResult()
    {
        var menuItem = CreateMenuItemWithNutritionalInfo();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);
        await _service.HandleAsync(menuItem.Id);

        menuItem.NutritionalInfo.Should().BeNull();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuItem = CreateMenuItemWithNutritionalInfo();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuItem = CreateMenuItemWithNutritionalInfo();
        SetupRepositoryGet(menuItem);

        await _service.HandleAsync(menuItem.Id);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetsMenuItemBeforeSaving()
    {
        var menuItem = CreateMenuItemWithNutritionalInfo();
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
        var serviceMock = new Mock<RemoveMenuItemNutritionalInfo.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuItemId)).Returns(Task.CompletedTask);

        var result = await RemoveMenuItemNutritionalInfo.Handler(serviceMock.Object, menuItemId);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithId()
    {
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveMenuItemNutritionalInfo.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await RemoveMenuItemNutritionalInfo.Handler(serviceMock.Object, menuItemId);

        serviceMock.Verify(s => s.HandleAsync(menuItemId), Times.Once);
    }

    #endregion
}
