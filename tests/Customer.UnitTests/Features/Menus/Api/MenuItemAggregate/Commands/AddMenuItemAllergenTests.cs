namespace Customer.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class AddMenuItemAllergenTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly AllergenValidator _allergenValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly Allergen.Create _allergenCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.AddAllergen _addAllergen;
    private readonly Mock<AddMenuItemAllergen.IRepository> _repositoryMock;
    private readonly Mock<IEntityLookup> _entityLookupMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddMenuItemAllergen.Service _service;

    public AddMenuItemAllergenTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _allergenCreate = new(_allergenValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _addAllergen = new(_menuItemValidator);
        _repositoryMock = new Mock<AddMenuItemAllergen.IRepository>();
        _entityLookupMock = new Mock<IEntityLookup>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new AddMenuItemAllergen.Service(
            _addAllergen,
            _repositoryMock.Object,
            _entityLookupMock.Object,
            _unitOfWorkMock.Object);
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

    private Allergen CreateAllergen(string code = "GLUTEN", string name = "Gluten")
    {
        var command = new CreateAllergenCommand(
            Code: code,
            Name: name,
            IconUrl: null,
            IsActive: true,
            DisplayOrder: 0
        );
        return _allergenCreate.Execute(command);
    }

    private void SetupRepositoryGet(MenuItemAgg menuItem)
    {
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);
    }

    private void SetupEntityLookupGet(Allergen allergen)
    {
        _entityLookupMock.Setup(r => r.GetRequiredAsync<Allergen, string>(
            allergen.Id,
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).ReturnsAsync(allergen);
    }

    private static AddMenuItemAllergen.Request CreateValidRequest(string allergenId = "GLUTEN")
    {
        return new AddMenuItemAllergen.Request(AllergenId: allergenId);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_AddsAllergen()
    {
        var menuItem = CreateMenuItem();
        var allergen = CreateAllergen("GLUTEN", "Gluten");
        SetupRepositoryGet(menuItem);
        SetupEntityLookupGet(allergen);
        var request = CreateValidRequest("GLUTEN");

        var response = await _service.HandleAsync(menuItem.Id, request);

        menuItem.Allergens.Should().ContainSingle();
        menuItem.Allergens.Should().Contain(a => a.Id == "GLUTEN");
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_WithMultipleAllergens_AddsAllAllergens()
    {
        var menuItem = CreateMenuItem();
        var glutenAllergen = CreateAllergen("GLUTEN", "Gluten");
        var lactoseAllergen = CreateAllergen("LACTOSE", "Lactosa");
        SetupRepositoryGet(menuItem);
        _entityLookupMock.Setup(r => r.GetRequiredAsync<Allergen, string>(
            "GLUTEN", It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(glutenAllergen);
        _entityLookupMock.Setup(r => r.GetRequiredAsync<Allergen, string>(
            "LACTOSE", It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(lactoseAllergen);

        await _service.HandleAsync(menuItem.Id, CreateValidRequest("GLUTEN"));
        await _service.HandleAsync(menuItem.Id, CreateValidRequest("LACTOSE"));

        menuItem.Allergens.Should().HaveCount(2);
        menuItem.Allergens.Should().Contain(a => a.Id == "GLUTEN");
        menuItem.Allergens.Should().Contain(a => a.Id == "LACTOSE");
    }

    [Fact]
    public async Task HandleAsync_ReturnsMenuItemResponse()
    {
        var menuItem = CreateMenuItem();
        var allergen = CreateAllergen("GLUTEN", "Gluten");
        SetupRepositoryGet(menuItem);
        SetupEntityLookupGet(allergen);
        var request = CreateValidRequest("GLUTEN");

        var response = await _service.HandleAsync(menuItem.Id, request);

        response.Id.Should().Be(menuItem.Id);
        response.Allergens.Should().ContainSingle();
        response.Allergens.Should().Contain(a => a.Id == "GLUTEN");
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
        var allergen = CreateAllergen();
        SetupRepositoryGet(menuItem);
        SetupEntityLookupGet(allergen);
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
        var allergen = CreateAllergen();
        SetupRepositoryGet(menuItem);
        SetupEntityLookupGet(allergen);
        var request = CreateValidRequest();

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
        var allergen = CreateAllergen();
        SetupRepositoryGet(menuItem);
        SetupEntityLookupGet(allergen);
        var request = CreateValidRequest();

        await _service.HandleAsync(menuItem.Id, request);

        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsEntityLookupGetRequiredAsync()
    {
        var menuItem = CreateMenuItem();
        var allergen = CreateAllergen("GLUTEN");
        SetupRepositoryGet(menuItem);
        SetupEntityLookupGet(allergen);
        var request = CreateValidRequest("GLUTEN");

        await _service.HandleAsync(menuItem.Id, request);

        _entityLookupMock.Verify(r => r.GetRequiredAsync<Allergen, string>(
            "GLUTEN", It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuItem = CreateMenuItem();
        var allergen = CreateAllergen();
        SetupRepositoryGet(menuItem);
        SetupEntityLookupGet(allergen);
        var request = CreateValidRequest();

        await _service.HandleAsync(menuItem.Id, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetsMenuItemBeforeSaving()
    {
        var menuItem = CreateMenuItem();
        var allergen = CreateAllergen();
        var callOrder = new List<string>();
        _repositoryMock.Setup(r => r.Get(menuItem.Id))
            .Callback(() => callOrder.Add("Get"))
            .ReturnsAsync(menuItem);
        SetupEntityLookupGet(allergen);
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
    public async Task HandleAsync_WithDuplicateAllergen_ThrowsConflictException()
    {
        var menuItem = CreateMenuItem();
        var allergen = CreateAllergen("GLUTEN", "Gluten");
        SetupRepositoryGet(menuItem);
        SetupEntityLookupGet(allergen);

        await _service.HandleAsync(menuItem.Id, CreateValidRequest("GLUTEN"));

        var act = () => _service.HandleAsync(menuItem.Id, CreateValidRequest("GLUTEN"));

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Allergen already exists in this item");
    }

    [Fact]
    public async Task HandleAsync_WhenConflictOccurs_DoesNotCallUnitOfWorkAgain()
    {
        var menuItem = CreateMenuItem();
        var allergen = CreateAllergen("GLUTEN", "Gluten");
        SetupRepositoryGet(menuItem);
        SetupEntityLookupGet(allergen);

        await _service.HandleAsync(menuItem.Id, CreateValidRequest("GLUTEN"));
        _unitOfWorkMock.Invocations.Clear();

        try { await _service.HandleAsync(menuItem.Id, CreateValidRequest("GLUTEN")); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        var act = () => _service.HandleAsync(nonExistentId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenAllergenNotFound_ThrowsKeyNotFoundException()
    {
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);
        _entityLookupMock.Setup(r => r.GetRequiredAsync<Allergen, string>(
            "NON_EXISTENT", It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ThrowsAsync(new KeyNotFoundException("Allergen not found"));
        var request = CreateValidRequest("NON_EXISTENT");

        var act = () => _service.HandleAsync(menuItem.Id, request);

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
    public async Task Handler_WithValidRequest_ReturnsCreatedResult()
    {
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<AddMenuItemAllergen.IService>();
        var response = new MenuItemResponse(
            menuItemId, _tenantId, "Test", null, null, 0, true, true, false, false, null,
            true, null, true, true, false, null, null, [], [], []);

        serviceMock.Setup(s => s.HandleAsync(menuItemId, request)).ReturnsAsync(response);

        var result = await AddMenuItemAllergen.Handler(serviceMock.Object, menuItemId, request);

        result.Should().BeOfType<Created<MenuItemResponse>>();
        var created = (Created<MenuItemResponse>)result;
        created.Location.Should().Be($"/menu-items/{menuItemId}");
    }

    [Fact]
    public async Task Handler_CallsServiceWithIdAndRequest()
    {
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<AddMenuItemAllergen.IService>();
        var response = new MenuItemResponse(
            menuItemId, _tenantId, "Test", null, null, 0, true, true, false, false, null,
            true, null, true, true, false, null, null, [], [], []);

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<AddMenuItemAllergen.Request>()))
            .ReturnsAsync(response);

        await AddMenuItemAllergen.Handler(serviceMock.Object, menuItemId, request);

        serviceMock.Verify(s => s.HandleAsync(menuItemId, request), Times.Once);
    }

    #endregion
}
