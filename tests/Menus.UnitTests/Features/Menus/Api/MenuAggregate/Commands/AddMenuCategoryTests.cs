namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class AddMenuCategoryTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly Mock<AddMenuCategory.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddMenuCategory.Service _service;

    public AddMenuCategoryTests()
    {
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        _createMenu = new(_menuValidator);
        _addCategory = new(createCategory, _menuValidator);

        _repositoryMock = new Mock<AddMenuCategory.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new AddMenuCategory.Service(_addCategory, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private MenuAgg CreateMenu()
    {
        return _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Test Menu"
        ));
    }

    private MenuAgg CreateMenuWithCategory(string categoryName)
    {
        var menu = CreateMenu();
        _addCategory.Execute(menu, new AddCategoryCommand(categoryName));
        return menu;
    }

    private static AddMenuCategory.Request CreateValidRequest(
        string name = "Entrantes",
        string? description = null,
        int displayOrder = 0)
    {
        return new AddMenuCategory.Request(
            Name: name,
            Description: description,
            DisplayOrder: displayOrder
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_AddsCategoryToMenu()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        var response = await _service.HandleAsync(menuId, request);

        menu.Categories.Should().HaveCount(1);
        menu.Categories.First().Name.Should().Be("Entrantes");
    }

    [Fact]
    public async Task HandleAsync_ReturnsMenuResponse()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        var response = await _service.HandleAsync(menuId, request);

        response.Should().NotBeNull();
        response.Categories.Should().HaveCount(1);
        response.Categories.First().Name.Should().Be("Entrantes");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_CreatesCategoryWithAllProperties()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            name: "Postres",
            description: "Deliciosos postres caseros",
            displayOrder: 5
        );

        var response = await _service.HandleAsync(menuId, request);

        var category = menu.Categories.First();
        category.Name.Should().Be("Postres");
        category.Description.Should().Be("Deliciosos postres caseros");
        category.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_CreatesCategoryAsActive()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        await _service.HandleAsync(menuId, request);

        menu.Categories.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_CreatesCategoryWithEmptyItems()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        await _service.HandleAsync(menuId, request);

        menu.Categories.First().Items.Should().BeEmpty();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        await _service.HandleAsync(menuId, request);

        _repositoryMock.Verify(r => r.Get(menuId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        await _service.HandleAsync(menuId, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMenuNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ThrowsAsync(new KeyNotFoundException());

        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task HandleAsync_WithDuplicateCategoryName_ThrowsConflictException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory("Entrantes");
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: "Entrantes");

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateCategoryNameCaseInsensitive_ThrowsConflictException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory("Entrantes");
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: "ENTRANTES");

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException(string name)
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: name);

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNameExceeding100Characters_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: new string('a', 101));

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithDescriptionExceeding500Characters_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(description: new string('a', 501));

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(displayOrder: -1);

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: "");

        try { await _service.HandleAsync(menuId, request); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsCreatedResult()
    {
        var menuId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<AddMenuCategory.IService>();
        var expectedResponse = new MenuResponse(
            Id: menuId,
            Name: "Test Menu",
            Description: null,
            IsActive: false,
            DisplayOrder: 0,
            EffectiveFrom: null,
            EffectiveUntil: null,
            DepositPolicy: null,
            Categories: [new MenuCategoryResponse(Guid.NewGuid(), "Entrantes", null, 0, true, [])]
        );

        serviceMock.Setup(s => s.HandleAsync(menuId, request)).ReturnsAsync(expectedResponse);

        var result = await AddMenuCategory.Handler(serviceMock.Object, menuId, request);

        result.Should().BeOfType<Created<MenuResponse>>();
        var createdResult = (Created<MenuResponse>)result;
        createdResult.Location.Should().Be($"/menus/{expectedResponse.Id}");
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var menuId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<AddMenuCategory.IService>();
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

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<AddMenuCategory.Request>())).ReturnsAsync(expectedResponse);

        await AddMenuCategory.Handler(serviceMock.Object, menuId, request);

        serviceMock.Verify(s => s.HandleAsync(menuId, request), Times.Once);
    }

    #endregion
}
