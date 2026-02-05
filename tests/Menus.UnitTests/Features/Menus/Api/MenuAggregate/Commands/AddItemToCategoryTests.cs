namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class AddItemToCategoryTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _categoryItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.AddItemToCategory _addItemToCategory;
    private readonly Mock<AddItemToCategory.IRepository> _repositoryMock;
    private readonly Mock<IEntityLookup> _entityLookupMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddItemToCategory.Service _service;

    public AddItemToCategoryTests()
    {
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        var createCategoryItem = new CategoryItemVO.Create(_categoryItemValidator);
        var addItem = new MenuCategoryEntity.AddItem(createCategoryItem, _categoryValidator);
        var createPriceOption = new PriceOptionVO.Create(_priceOptionValidator);

        _createMenu = new(_menuValidator);
        _addCategory = new(createCategory, _menuValidator);
        _addItemToCategory = new(addItem, createPriceOption, _menuValidator);

        _repositoryMock = new Mock<AddItemToCategory.IRepository>();
        _entityLookupMock = new Mock<IEntityLookup>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new AddItemToCategory.Service(
            _addItemToCategory,
            _repositoryMock.Object,
            _entityLookupMock.Object,
            _unitOfWorkMock.Object
        );
    }

    private MenuAgg CreateMenuWithCategory()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Test Menu"
        ));
        _addCategory.Execute(menu, new AddCategoryCommand("Entrantes"));
        return menu;
    }

    private MenuItemAgg CreateMenuItem(Guid? id = null)
    {
        var createPriceOption = new PriceOptionVO.Create(_priceOptionValidator);
        var create = new MenuItemAgg.Create(createPriceOption, _menuItemValidator);

        return create.Execute(new CreateMenuItemCommand(
            TenantId: _tenantId,
            Name: "Test Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, 15.00m)]
        ));
    }

    private static AddItemToCategory.Request CreateValidRequest(
        Guid menuItemId,
        int displayOrder = 0,
        AddItemToCategory.PriceOptionData[]? priceOverrides = null)
    {
        return new AddItemToCategory.Request(
            MenuItemId: menuItemId,
            DisplayOrder: displayOrder,
            PriceOverrides: priceOverrides
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_AddsItemToCategory()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        var menuItem = CreateMenuItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);
        _entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>())).ReturnsAsync(menuItem);

        var request = CreateValidRequest(menuItem.Id);

        var response = await _service.HandleAsync(menuId, categoryId, request);

        menu.Categories.First().Items.Should().HaveCount(1);
        menu.Categories.First().Items.First().MenuItem.Id.Should().Be(menuItem.Id);
    }

    [Fact]
    public async Task HandleAsync_ReturnsMenuResponse()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        var menuItem = CreateMenuItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);
        _entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>())).ReturnsAsync(menuItem);

        var request = CreateValidRequest(menuItem.Id);

        var response = await _service.HandleAsync(menuId, categoryId, request);

        response.Should().NotBeNull();
        response.Categories.First().Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_WithDisplayOrder_SetsDisplayOrder()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        var menuItem = CreateMenuItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);
        _entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>())).ReturnsAsync(menuItem);

        var request = CreateValidRequest(menuItem.Id, displayOrder: 5);

        await _service.HandleAsync(menuId, categoryId, request);

        menu.Categories.First().Items.First().DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_WithPriceOverrides_SetsPriceOverrides()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        var menuItem = CreateMenuItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);
        _entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>())).ReturnsAsync(menuItem);

        var request = CreateValidRequest(
            menuItem.Id,
            priceOverrides: [new AddItemToCategory.PriceOptionData(PortionType.Full, 20.00m, true)]
        );

        await _service.HandleAsync(menuId, categoryId, request);

        var item = menu.Categories.First().Items.First();
        item.PriceOverrides.Should().HaveCount(1);
        item.PriceOverrides.First().Price.Should().Be(20.00m);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        var menuItem = CreateMenuItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);
        _entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>())).ReturnsAsync(menuItem);

        var request = CreateValidRequest(menuItem.Id);

        await _service.HandleAsync(menuId, categoryId, request);

        _repositoryMock.Verify(r => r.Get(menuId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsEntityLookup()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        var menuItem = CreateMenuItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);
        _entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>())).ReturnsAsync(menuItem);

        var request = CreateValidRequest(menuItem.Id);

        await _service.HandleAsync(menuId, categoryId, request);

        _entityLookupMock.Verify(e => e.GetRequiredAsync<MenuItemAgg, Guid>(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        var menuItem = CreateMenuItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);
        _entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>())).ReturnsAsync(menuItem);

        var request = CreateValidRequest(menuItem.Id);

        await _service.HandleAsync(menuId, categoryId, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMenuNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ThrowsAsync(new KeyNotFoundException());

        var request = CreateValidRequest(Guid.NewGuid());

        var act = () => _service.HandleAsync(menuId, categoryId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenMenuItemNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        var menuItemId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);
        _entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>())).ThrowsAsync(new KeyNotFoundException());

        var request = CreateValidRequest(menuItemId);

        var act = () => _service.HandleAsync(menuId, categoryId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenCategoryNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var nonExistentCategoryId = Guid.NewGuid();
        var menuItem = CreateMenuItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);
        _entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>())).ReturnsAsync(menuItem);

        var request = CreateValidRequest(menuItem.Id);

        var act = () => _service.HandleAsync(menuId, nonExistentCategoryId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task HandleAsync_WhenItemAlreadyInCategory_ThrowsConflictException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        var menuItem = CreateMenuItem();
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem));
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);
        _entityLookupMock.Setup(e => e.GetRequiredAsync<MenuItemAgg, Guid>(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>())).ReturnsAsync(menuItem);

        var request = CreateValidRequest(menuItem.Id);

        var act = () => _service.HandleAsync(menuId, categoryId, request);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsCreatedResult()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest(menuItemId);
        var serviceMock = new Mock<AddItemToCategory.IService>();
        var expectedResponse = new MenuResponse(
            Id: menuId,
            Name: "Test Menu",
            Description: null,
            IsActive: false,
            DisplayOrder: 0,
            EffectiveFrom: null,
            EffectiveUntil: null,
            DepositPolicy: null,
            Categories: []
        );

        serviceMock.Setup(s => s.HandleAsync(menuId, categoryId, request)).ReturnsAsync(expectedResponse);

        var result = await AddItemToCategory.Handler(serviceMock.Object, menuId, categoryId, request);

        result.Should().BeOfType<Created<MenuResponse>>();
        var createdResult = (Created<MenuResponse>)result;
        createdResult.Location.Should().Be($"/menus/{expectedResponse.Id}");
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest(menuItemId);
        var serviceMock = new Mock<AddItemToCategory.IService>();
        var expectedResponse = new MenuResponse(
            Id: menuId,
            Name: "Test",
            Description: null,
            IsActive: false,
            DisplayOrder: 0,
            EffectiveFrom: null,
            EffectiveUntil: null,
            DepositPolicy: null,
            Categories: []
        );

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AddItemToCategory.Request>())).ReturnsAsync(expectedResponse);

        await AddItemToCategory.Handler(serviceMock.Object, menuId, categoryId, request);

        serviceMock.Verify(s => s.HandleAsync(menuId, categoryId, request), Times.Once);
    }

    #endregion
}
