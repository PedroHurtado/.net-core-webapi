namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class UpdateMenuCategoryTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.UpdateCategory _updateCategory;
    private readonly Mock<UpdateMenuCategory.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateMenuCategory.Service _service;

    public UpdateMenuCategoryTests()
    {
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        var updateCategoryDetails = new MenuCategoryEntity.Update(_categoryValidator);
        _createMenu = new(_menuValidator);
        _addCategory = new(createCategory, _menuValidator);
        _updateCategory = new(updateCategoryDetails, _menuValidator);

        _repositoryMock = new Mock<UpdateMenuCategory.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UpdateMenuCategory.Service(_updateCategory, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private MenuAgg CreateMenuWithCategory(string categoryName = "Entrantes")
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Test Menu"
        ));
        _addCategory.Execute(menu, new AddCategoryCommand(categoryName));
        return menu;
    }

    private MenuAgg CreateMenuWithTwoCategories()
    {
        var menu = CreateMenuWithCategory("Entrantes");
        _addCategory.Execute(menu, new AddCategoryCommand("Postres"));
        return menu;
    }

    private static UpdateMenuCategory.Request CreateValidRequest(
        string name = "Updated Category",
        string? description = null,
        int displayOrder = 0)
    {
        return new UpdateMenuCategory.Request(
            Name: name,
            Description: description,
            DisplayOrder: displayOrder
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesCategory()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: "New Name");

        await _service.HandleAsync(menuId, categoryId, request);

        menu.Categories.First().Name.Should().Be("New Name");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_UpdatesAllProperties()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            name: "Categoría Actualizada",
            description: "Nueva descripción",
            displayOrder: 10
        );

        await _service.HandleAsync(menuId, categoryId, request);

        var category = menu.Categories.First();
        category.Name.Should().Be("Categoría Actualizada");
        category.Description.Should().Be("Nueva descripción");
        category.DisplayOrder.Should().Be(10);
    }

    [Fact]
    public async Task HandleAsync_PreservesIsActiveStatus()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        var originalIsActive = menu.Categories.First().IsActive;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        await _service.HandleAsync(menuId, categoryId, request);

        menu.Categories.First().IsActive.Should().Be(originalIsActive);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        await _service.HandleAsync(menuId, categoryId, request);

        _repositoryMock.Verify(r => r.Get(menuId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        await _service.HandleAsync(menuId, categoryId, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMenuNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(menuId)).ThrowsAsync(new KeyNotFoundException());

        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(menuId, categoryId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenCategoryNotFound_ThrowsKeyNotFoundException()
    {
        var menuId = Guid.NewGuid();
        var nonExistentCategoryId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(menuId, nonExistentCategoryId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task HandleAsync_WithDuplicateCategoryName_ThrowsConflictException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithTwoCategories();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: "Postres");

        var act = () => _service.HandleAsync(menuId, categoryId, request);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task HandleAsync_WithSameNameAsItself_Succeeds()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory("Entrantes");
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: "Entrantes", description: "Updated description");

        await _service.HandleAsync(menuId, categoryId, request);

        menu.Categories.First().Description.Should().Be("Updated description");
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException(string name)
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: name);

        var act = () => _service.HandleAsync(menuId, categoryId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNameExceeding100Characters_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: new string('a', 101));

        var act = () => _service.HandleAsync(menuId, categoryId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(displayOrder: -1);

        var act = () => _service.HandleAsync(menuId, categoryId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithCategory();
        var categoryId = menu.Categories.First().Id;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: "");

        try { await _service.HandleAsync(menuId, categoryId, request); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateMenuCategory.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuId, categoryId, request)).Returns(Task.CompletedTask);

        var result = await UpdateMenuCategory.Handler(serviceMock.Object, menuId, categoryId, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var menuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateMenuCategory.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateMenuCategory.Request>())).Returns(Task.CompletedTask);

        await UpdateMenuCategory.Handler(serviceMock.Object, menuId, categoryId, request);

        serviceMock.Verify(s => s.HandleAsync(menuId, categoryId, request), Times.Once);
    }

    #endregion
}
