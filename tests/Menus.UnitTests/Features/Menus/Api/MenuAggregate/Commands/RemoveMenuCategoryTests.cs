namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class RemoveMenuCategoryTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _categoryItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.RemoveCategory _removeCategory;
    private readonly MenuAgg.AddItemToCategory _addItemToCategory;
    private readonly Mock<RemoveMenuCategory.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveMenuCategory.Service _service;

    public RemoveMenuCategoryTests()
    {
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        var createCategoryItem = new CategoryItemVO.Create(_categoryItemValidator);
        var addItem = new MenuCategoryEntity.AddItem(createCategoryItem, _categoryValidator);
        var createPriceOption = new PriceOptionVO.Create(_priceOptionValidator);

        _createMenu = new(_menuValidator);
        _addCategory = new(createCategory, _menuValidator);
        _removeCategory = new(_menuValidator);
        _addItemToCategory = new(addItem, createPriceOption, _menuValidator);

        _repositoryMock = new Mock<RemoveMenuCategory.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new RemoveMenuCategory.Service(_removeCategory, _repositoryMock.Object, _unitOfWorkMock.Object);
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

    private MenuAgg CreateMenuWithCategoryContainingItems()
    {
        var menu = CreateMenuWithEmptyCategory();
        var categoryId = menu.Categories.First().Id;
        var menuItem = CreateMenuItem();
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem));
        return menu;
    }

    private static MenuItemAgg CreateMenuItem()
    {
        var validator = new MenuItemValidator();
        var priceOptionValidator = new PriceOptionValidator();
        var createPriceOption = new PriceOptionVO.Create(priceOptionValidator);
        var create = new MenuItemAgg.Create(createPriceOption, validator);

        return create.Execute(new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
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
            PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, 10.00m)]
        ));
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithEmptyCategory_RemovesCategoryFromMenu()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithEmptyCategory();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        await _service.HandleAsync(menuId, categoryId);

        menu.Categories.Should().BeEmpty();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithEmptyCategory();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        await _service.HandleAsync(menuId, categoryId);

        _repositoryMock.Verify(r => r.Get(menuId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithEmptyCategory();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        await _service.HandleAsync(menuId, categoryId);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMenuNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(menuId, categoryId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenCategoryNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var nonExistentCategoryId = Guid.NewGuid();
        var menu = CreateMenuWithEmptyCategory();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var act = () => _service.HandleAsync(menuId, nonExistentCategoryId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_WhenCategoryHasItems_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategoryContainingItems();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var act = () => _service.HandleAsync(menuId, categoryId);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*contains items*");
    }

    [Fact]
    public async Task HandleAsync_WhenRemovalFails_DoesNotCallUnitOfWork()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategoryContainingItems();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        try { await _service.HandleAsync(menuId, categoryId); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveMenuCategory.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuId, categoryId)).Returns(Task.CompletedTask);

        var result = await RemoveMenuCategory.Handler(serviceMock.Object, menuId, categoryId);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveMenuCategory.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await RemoveMenuCategory.Handler(serviceMock.Object, menuId, categoryId);

        serviceMock.Verify(s => s.HandleAsync(menuId, categoryId), Times.Once);
    }

    #endregion
}
