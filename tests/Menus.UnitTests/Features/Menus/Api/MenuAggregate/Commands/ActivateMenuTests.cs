namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class ActivateMenuTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _categoryItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.Activate _activateMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.AddItemToCategory _addItemToCategory;
    private readonly Mock<ActivateMenu.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ActivateMenu.Service _service;

    public ActivateMenuTests()
    {
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        var createCategoryItem = new CategoryItemVO.Create(_categoryItemValidator);
        var addItem = new MenuCategoryEntity.AddItem(createCategoryItem, _categoryValidator);
        var createPriceOption = new PriceOptionVO.Create(_priceOptionValidator);

        _createMenu = new(_menuValidator);
        _activateMenu = new(_menuValidator);
        _addCategory = new(createCategory, _menuValidator);
        _addItemToCategory = new(addItem, createPriceOption, _menuValidator);

        _repositoryMock = new Mock<ActivateMenu.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new ActivateMenu.Service(_activateMenu, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private MenuAgg CreateMenuWithCategoryAndItem()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Test Menu"
        ));

        _addCategory.Execute(menu, new AddCategoryCommand("Entrantes"));

        var menuItem = CreateMenuItem();
        var categoryId = menu.Categories.First().Id;
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem));

        return menu;
    }

    private MenuAgg CreateInactiveMenuWithoutCategories()
    {
        return _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Empty Menu"
        ));
    }

    private MenuAgg CreateInactiveMenuWithEmptyCategory()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Menu with Empty Category"
        ));

        _addCategory.Execute(menu, new AddCategoryCommand("Empty Category"));

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
    public async Task HandleAsync_WithInactiveMenuWithCategoryAndItems_ActivatesMenu()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategoryAndItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var response = await _service.HandleAsync(menuId);

        response.IsActive.Should().BeTrue();
        menu.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ReturnsMenuResponse()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategoryAndItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var response = await _service.HandleAsync(menuId);

        response.Should().NotBeNull();
        response.Name.Should().Be("Test Menu");
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategoryAndItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        await _service.HandleAsync(menuId);

        _repositoryMock.Verify(r => r.Get(menuId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategoryAndItem();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        await _service.HandleAsync(menuId);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMenuNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(menuId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task HandleAsync_WhenMenuAlreadyActive_ThrowsConflictException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategoryAndItem();
        _activateMenu.Execute(menu);
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var act = () => _service.HandleAsync(menuId);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already active*");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task HandleAsync_WhenMenuHasNoCategories_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateInactiveMenuWithoutCategories();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var act = () => _service.HandleAsync(menuId);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one category*");
    }

    [Fact]
    public async Task HandleAsync_WhenMenuHasNoCategoriesWithItems_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateInactiveMenuWithEmptyCategory();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var act = () => _service.HandleAsync(menuId);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one category with items*");
    }

    [Fact]
    public async Task HandleAsync_WhenActivationFails_DoesNotCallUnitOfWork()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateInactiveMenuWithoutCategories();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        try { await _service.HandleAsync(menuId); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsOkResult()
    {
        var menuId = Guid.NewGuid();
        var serviceMock = new Mock<ActivateMenu.IService>();
        var expectedResponse = new MenuResponse(
            Id: menuId,
            Name: "Test Menu",
            Description: null,
            IsActive: true,
            DisplayOrder: 0,
            EffectiveFrom: null,
            EffectiveUntil: null,
            DepositPolicy: null,
            Categories: []
        );

        serviceMock.Setup(s => s.HandleAsync(menuId)).ReturnsAsync(expectedResponse);

        var result = await ActivateMenu.Handler(serviceMock.Object, menuId);

        result.Should().BeOfType<Ok<MenuResponse>>();
        var okResult = (Ok<MenuResponse>)result;
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectId()
    {
        var menuId = Guid.NewGuid();
        var serviceMock = new Mock<ActivateMenu.IService>();
        var expectedResponse = new MenuResponse(
            Id: menuId,
            Name: "Test",
            Description: null,
            IsActive: true,
            DisplayOrder: 0,
            EffectiveFrom: null,
            EffectiveUntil: null,
            DepositPolicy: null,
            Categories: []
        );

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>())).ReturnsAsync(expectedResponse);

        await ActivateMenu.Handler(serviceMock.Object, menuId);

        serviceMock.Verify(s => s.HandleAsync(menuId), Times.Once);
    }

    #endregion
}
