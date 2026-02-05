namespace Menus.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class SetMenuItemNutritionalInfoTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly NutritionalInfoValidator _nutritionalInfoValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly NutritionalInfoVO.Create _nutritionalInfoCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.SetNutritionalInfo _setNutritionalInfo;
    private readonly Mock<SetMenuItemNutritionalInfo.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SetMenuItemNutritionalInfo.Service _service;

    public SetMenuItemNutritionalInfoTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _nutritionalInfoCreate = new(_nutritionalInfoValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _setNutritionalInfo = new(_nutritionalInfoCreate, _menuItemValidator);
        _repositoryMock = new Mock<SetMenuItemNutritionalInfo.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new SetMenuItemNutritionalInfo.Service(_setNutritionalInfo, _repositoryMock.Object, _unitOfWorkMock.Object);
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

    private static SetMenuItemNutritionalInfo.Request CreateValidRequest(
        int calories = 500,
        decimal protein = 25.0m,
        decimal carbohydrates = 40.0m,
        decimal fat = 15.0m,
        int servingSize = 350,
        decimal? fiber = null,
        decimal? sugar = null,
        decimal? salt = null)
    {
        return new SetMenuItemNutritionalInfo.Request(
            Calories: calories,
            Protein: protein,
            Carbohydrates: carbohydrates,
            Fat: fat,
            ServingSize: servingSize,
            Fiber: fiber,
            Sugar: sugar,
            Salt: salt
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_SetsNutritionalInfo()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(
            calories: 500,
            protein: 25.0m,
            carbohydrates: 40.0m,
            fat: 15.0m,
            servingSize: 350
        );

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.NutritionalInfo.Should().NotBeNull();
        menuItem.NutritionalInfo!.Calories.Should().Be(500);
        menuItem.NutritionalInfo.Protein.Should().Be(25.0m);
        menuItem.NutritionalInfo.Carbohydrates.Should().Be(40.0m);
        menuItem.NutritionalInfo.Fat.Should().Be(15.0m);
        menuItem.NutritionalInfo.ServingSize.Should().Be(350);
    }

    [Fact]
    public async Task HandleAsync_WithOptionalFields_SetsAllFields()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(
            fiber: 5.0m,
            sugar: 8.0m,
            salt: 1.5m
        );

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.NutritionalInfo!.Fiber.Should().Be(5.0m);
        menuItem.NutritionalInfo.Sugar.Should().Be(8.0m);
        menuItem.NutritionalInfo.Salt.Should().Be(1.5m);
    }

    [Fact]
    public async Task HandleAsync_WithoutOptionalFields_LeavesOptionalFieldsNull()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(
            fiber: null,
            sugar: null,
            salt: null
        );

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.NutritionalInfo!.Fiber.Should().BeNull();
        menuItem.NutritionalInfo.Sugar.Should().BeNull();
        menuItem.NutritionalInfo.Salt.Should().BeNull();
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
    public async Task HandleAsync_ReplacesExistingNutritionalInfo()
    {
        var menuItem = CreateMenuItem();
        _setNutritionalInfo.Execute(menuItem, new SetNutritionalInfoCommand(300, 15.0m, 30.0m, 10.0m, 250));
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(
            calories: 600,
            protein: 35.0m,
            carbohydrates: 50.0m,
            fat: 20.0m,
            servingSize: 400
        );

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.NutritionalInfo!.Calories.Should().Be(600);
        menuItem.NutritionalInfo.Protein.Should().Be(35.0m);
        menuItem.NutritionalInfo.Carbohydrates.Should().Be(50.0m);
        menuItem.NutritionalInfo.Fat.Should().Be(20.0m);
        menuItem.NutritionalInfo.ServingSize.Should().Be(400);
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
    public async Task HandleAsync_WithNegativeCalories_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(calories: -1);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithCaloriesExceedingMax_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(calories: 10001);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithZeroServingSize_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(servingSize: 0);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativeProtein_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(protein: -1);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativeSalt_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(salt: -1);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(calories: -1);

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
        var serviceMock = new Mock<SetMenuItemNutritionalInfo.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuItemId, request)).Returns(Task.CompletedTask);

        var result = await SetMenuItemNutritionalInfo.Handler(serviceMock.Object, menuItemId, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithIdAndRequest()
    {
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<SetMenuItemNutritionalInfo.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<SetMenuItemNutritionalInfo.Request>())).Returns(Task.CompletedTask);

        await SetMenuItemNutritionalInfo.Handler(serviceMock.Object, menuItemId, request);

        serviceMock.Verify(s => s.HandleAsync(menuItemId, request), Times.Once);
    }

    #endregion
}
