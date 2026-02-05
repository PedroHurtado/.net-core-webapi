namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class DeactivateMenuTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _categoryItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.Activate _activateMenu;
    private readonly MenuAgg.Deactivate _deactivateMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.AddItemToCategory _addItemToCategory;
    private readonly Mock<DeactivateMenu.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateMenu.Service _service;

    public DeactivateMenuTests()
    {
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        var createCategoryItem = new CategoryItemVO.Create(_categoryItemValidator);
        var addItem = new MenuCategoryEntity.AddItem(createCategoryItem, _categoryValidator);
        var createPriceOption = new PriceOptionVO.Create(_priceOptionValidator);

        _createMenu = new(_menuValidator);
        _activateMenu = new(_menuValidator);
        _deactivateMenu = new(_menuValidator);
        _addCategory = new(createCategory, _menuValidator);
        _addItemToCategory = new(addItem, createPriceOption, _menuValidator);

        _repositoryMock = new Mock<DeactivateMenu.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new DeactivateMenu.Service(_deactivateMenu, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private MenuAgg CreateActiveMenu()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Active Menu"
        ));
        _addCategory.Execute(menu, new AddCategoryCommand("Category"));
        var menuItem = CreateMenuItem();
        var categoryId = menu.Categories.First().Id;
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem));
        _activateMenu.Execute(menu);
        return menu;
    }

    private MenuAgg CreateInactiveMenu()
    {
        return _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Inactive Menu"
        ));
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
    public async Task HandleAsync_WithActiveMenu_DeactivatesMenu()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateActiveMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var response = await _service.HandleAsync(menuId);

        response.IsActive.Should().BeFalse();
        menu.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ReturnsMenuResponse()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateActiveMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var response = await _service.HandleAsync(menuId);

        response.Should().NotBeNull();
        response.Name.Should().Be("Active Menu");
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateActiveMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        await _service.HandleAsync(menuId);

        _repositoryMock.Verify(r => r.Get(menuId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateActiveMenu();
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
    public async Task HandleAsync_WhenMenuAlreadyInactive_ThrowsConflictException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateInactiveMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var act = () => _service.HandleAsync(menuId);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already inactive*");
    }

    [Fact]
    public async Task HandleAsync_WhenDeactivationFails_DoesNotCallUnitOfWork()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateInactiveMenu();
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
        var serviceMock = new Mock<DeactivateMenu.IService>();
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

        serviceMock.Setup(s => s.HandleAsync(menuId)).ReturnsAsync(expectedResponse);

        var result = await DeactivateMenu.Handler(serviceMock.Object, menuId);

        result.Should().BeOfType<Ok<MenuResponse>>();
        var okResult = (Ok<MenuResponse>)result;
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectId()
    {
        var menuId = Guid.NewGuid();
        var serviceMock = new Mock<DeactivateMenu.IService>();
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

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>())).ReturnsAsync(expectedResponse);

        await DeactivateMenu.Handler(serviceMock.Object, menuId);

        serviceMock.Verify(s => s.HandleAsync(menuId), Times.Once);
    }

    #endregion
}
