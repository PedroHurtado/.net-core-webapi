namespace Customer.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class RemoveItemFromCategoryTests
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
    private readonly MenuAgg.RemoveItemFromCategory _removeItemFromCategory;
    private readonly Mock<RemoveItemFromCategory.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveItemFromCategory.Service _service;

    public RemoveItemFromCategoryTests()
    {
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        var createCategoryItem = new CategoryItemVO.Create(_categoryItemValidator);
        var addItem = new MenuCategoryEntity.AddItem(createCategoryItem, _categoryValidator);
        var removeItem = new MenuCategoryEntity.RemoveItem(_categoryValidator);
        var createPriceOption = new PriceOptionVO.Create(_priceOptionValidator);

        _createMenu = new(_menuValidator);
        _addCategory = new(createCategory, _menuValidator);
        _addItemToCategory = new(addItem, createPriceOption, _menuValidator);
        _removeItemFromCategory = new(removeItem, _menuValidator);

        _repositoryMock = new Mock<RemoveItemFromCategory.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new RemoveItemFromCategory.Service(_removeItemFromCategory, _repositoryMock.Object, _unitOfWorkMock.Object);
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

    private MenuAgg CreateMenuWithEmptyCategory()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Test Menu"
        ));
        _addCategory.Execute(menu, new AddCategoryCommand("Empty Category"));
        return menu;
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

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_RemovesItemFromCategory()
    {
        var menuId = Guid.NewGuid();
        var (menu, menuItem) = CreateMenuWithCategoryAndItem();
        var categoryId = menu.Categories.First().Id;
        menu.Categories.First().Items.Should().HaveCount(1);
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        await _service.HandleAsync(menuId, categoryId, menuItem.Id);

        menu.Categories.First().Items.Should().BeEmpty();
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

        await _service.HandleAsync(menuId, categoryId, menuItem.Id);

        _repositoryMock.Verify(r => r.Get(menuId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuId = Guid.NewGuid();
        var (menu, menuItem) = CreateMenuWithCategoryAndItem();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        await _service.HandleAsync(menuId, categoryId, menuItem.Id);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMenuNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(menuId, categoryId, menuItemId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenCategoryNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var (menu, menuItem) = CreateMenuWithCategoryAndItem();
        var nonExistentCategoryId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var act = () => _service.HandleAsync(menuId, nonExistentCategoryId, menuItem.Id);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenItemNotFoundInCategory_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithEmptyCategory();
        var categoryId = menu.Categories.First().Id;
        var nonExistentMenuItemId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var act = () => _service.HandleAsync(menuId, categoryId, nonExistentMenuItemId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenRemovalFails_DoesNotCallUnitOfWork()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithEmptyCategory();
        var categoryId = menu.Categories.First().Id;
        var nonExistentMenuItemId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        try { await _service.HandleAsync(menuId, categoryId, nonExistentMenuItemId); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveItemFromCategory.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuId, categoryId, menuItemId)).Returns(Task.CompletedTask);

        var result = await RemoveItemFromCategory.Handler(serviceMock.Object, menuId, categoryId, menuItemId);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveItemFromCategory.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await RemoveItemFromCategory.Handler(serviceMock.Object, menuId, categoryId, menuItemId);

        serviceMock.Verify(s => s.HandleAsync(menuId, categoryId, menuItemId), Times.Once);
    }

    #endregion
}
