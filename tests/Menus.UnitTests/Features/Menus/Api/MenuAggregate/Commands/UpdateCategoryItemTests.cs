namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class UpdateCategoryItemTests
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
    private readonly MenuAgg.UpdateCategoryItem _updateCategoryItem;
    private readonly Mock<UpdateCategoryItem.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateCategoryItem.Service _service;

    public UpdateCategoryItemTests()
    {
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        var createCategoryItem = new CategoryItemVO.Create(_categoryItemValidator);
        var addItem = new MenuCategoryEntity.AddItem(createCategoryItem, _categoryValidator);
        var updateItem = new MenuCategoryEntity.UpdateItem(createCategoryItem, _categoryValidator);
        var createPriceOption = new PriceOptionVO.Create(_priceOptionValidator);

        _createMenu = new(_menuValidator);
        _addCategory = new(createCategory, _menuValidator);
        _addItemToCategory = new(addItem, createPriceOption, _menuValidator);
        _updateCategoryItem = new(updateItem, createPriceOption, _menuValidator);

        _repositoryMock = new Mock<UpdateCategoryItem.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UpdateCategoryItem.Service(_updateCategoryItem, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private (MenuAgg menu, MenuItemAgg menuItem) CreateMenuWithCategoryAndItem()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Test Menu"
        ));
        _addCategory.Execute(menu, new AddCategoryCommand("Entrantes"));
        var menuItem = CreateMenuItem();
        var categoryId = menu.Categories.First().Id;
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem));
        return (menu, menuItem);
    }

    private MenuItemAgg CreateMenuItem()
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

    private static UpdateCategoryItem.Request CreateValidRequest(
        int displayOrder = 0,
        UpdateCategoryItem.PriceOptionData[]? priceOverrides = null)
    {
        return new UpdateCategoryItem.Request(
            DisplayOrder: displayOrder,
            PriceOverrides: priceOverrides
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesDisplayOrder()
    {
        var menuId = Guid.NewGuid();
        var (menu, menuItem) = CreateMenuWithCategoryAndItem();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(displayOrder: 10);

        await _service.HandleAsync(menuId, categoryId, menuItem.Id, request);

        menu.Categories.First().Items.First().DisplayOrder.Should().Be(10);
    }

    [Fact]
    public async Task HandleAsync_WithPriceOverrides_UpdatesPriceOverrides()
    {
        var menuId = Guid.NewGuid();
        var (menu, menuItem) = CreateMenuWithCategoryAndItem();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            priceOverrides: [new UpdateCategoryItem.PriceOptionData(PortionType.Full, 25.00m, true)]
        );

        await _service.HandleAsync(menuId, categoryId, menuItem.Id, request);

        var item = menu.Categories.First().Items.First();
        item.PriceOverrides.Should().HaveCount(1);
        item.PriceOverrides.First().Price.Should().Be(25.00m);
    }

    [Fact]
    public async Task HandleAsync_WithNullPriceOverrides_ClearsPriceOverrides()
    {
        var menuId = Guid.NewGuid();
        var (menu, menuItem) = CreateMenuWithCategoryAndItem();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(priceOverrides: null);

        await _service.HandleAsync(menuId, categoryId, menuItem.Id, request);

        var item = menu.Categories.First().Items.First();
        item.PriceOverrides.Should().BeEmpty();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuId = Guid.NewGuid();
        var (menu, menuItem) = CreateMenuWithCategoryAndItem();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        await _service.HandleAsync(menuId, categoryId, menuItem.Id, request);

        _repositoryMock.Verify(r => r.Get(menuId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuId = Guid.NewGuid();
        var (menu, menuItem) = CreateMenuWithCategoryAndItem();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        await _service.HandleAsync(menuId, categoryId, menuItem.Id, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMenuNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ThrowsAsync(new KeyNotFoundException());

        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(menuId, categoryId, menuItemId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenCategoryNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var (menu, menuItem) = CreateMenuWithCategoryAndItem();
        var nonExistentCategoryId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(menuId, nonExistentCategoryId, menuItem.Id, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenItemNotFoundInCategory_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var (menu, _) = CreateMenuWithCategoryAndItem();
        var categoryId = menu.Categories.First().Id;
        var nonExistentMenuItemId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(menuId, categoryId, nonExistentMenuItemId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateCategoryItem.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuId, categoryId, menuItemId, request)).Returns(Task.CompletedTask);

        var result = await UpdateCategoryItem.Handler(serviceMock.Object, menuId, categoryId, menuItemId, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateCategoryItem.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateCategoryItem.Request>())).Returns(Task.CompletedTask);

        await UpdateCategoryItem.Handler(serviceMock.Object, menuId, categoryId, menuItemId, request);

        serviceMock.Verify(s => s.HandleAsync(menuId, categoryId, menuItemId, request), Times.Once);
    }

    #endregion
}
