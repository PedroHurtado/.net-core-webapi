namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate.Queries;

public class GetMenuTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly Mock<GetMenu.IRepository> _repositoryMock;
    private readonly GetMenu.Service _service;

    public GetMenuTests()
    {
        _createMenu = new(_menuValidator);
        _repositoryMock = new Mock<GetMenu.IRepository>();
        _service = new GetMenu.Service(_repositoryMock.Object);
    }

    private MenuAgg CreateMenu(
        string name = "Test Menu",
        string? description = null,
        DateTime? effectiveFrom = null,
        DateTime? effectiveUntil = null)
    {
        return _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: name,
            Description: description,
            EffectiveFrom: effectiveFrom,
            EffectiveUntil: effectiveUntil
        ));
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithExistingId_ReturnsResponse()
    {
        var menu = CreateMenu(name: "Menú Degustación");
        _repositoryMock.Setup(r => r.Get(menu.Id)).ReturnsAsync(menu);

        var response = await _service.HandleAsync(menu.Id);

        response.Should().NotBeNull();
        response.Id.Should().Be(menu.Id);
        response.Name.Should().Be("Menú Degustación");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_ReturnsCompleteResponse()
    {
        var effectiveFrom = DateTime.UtcNow;
        var effectiveUntil = DateTime.UtcNow.AddMonths(3);
        var menu = CreateMenu(
            name: "Menú Navideño",
            description: "Menú especial para las fiestas",
            effectiveFrom: effectiveFrom,
            effectiveUntil: effectiveUntil
        );
        _repositoryMock.Setup(r => r.Get(menu.Id)).ReturnsAsync(menu);

        var response = await _service.HandleAsync(menu.Id);

        response.Id.Should().Be(menu.Id);
        response.Name.Should().Be("Menú Navideño");
        response.Description.Should().Be("Menú especial para las fiestas");
        response.EffectiveFrom.Should().Be(effectiveFrom);
        response.EffectiveUntil.Should().Be(effectiveUntil);
    }

    [Fact]
    public async Task HandleAsync_WithNullOptionalFields_ReturnsNullValues()
    {
        var menu = CreateMenu(
            name: "Simple Menu",
            description: null,
            effectiveFrom: null,
            effectiveUntil: null
        );
        _repositoryMock.Setup(r => r.Get(menu.Id)).ReturnsAsync(menu);

        var response = await _service.HandleAsync(menu.Id);

        response.Description.Should().BeNull();
        response.EffectiveFrom.Should().BeNull();
        response.EffectiveUntil.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithDefaultValues_ReturnsDefaults()
    {
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menu.Id)).ReturnsAsync(menu);

        var response = await _service.HandleAsync(menu.Id);

        response.IsActive.Should().BeFalse();
        response.DisplayOrder.Should().Be(0);
        response.Categories.Should().BeEmpty();
        response.DepositPolicy.Should().BeNull();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menu.Id)).ReturnsAsync(menu);

        await _service.HandleAsync(menu.Id);

        _repositoryMock.Verify(r => r.Get(menu.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PassesCorrectIdToRepository()
    {
        var menu = CreateMenu();
        var specificId = menu.Id;
        _repositoryMock.Setup(r => r.Get(specificId)).ReturnsAsync(menu);

        await _service.HandleAsync(specificId);

        _repositoryMock.Verify(r => r.Get(specificId), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task HandleAsync_WhenMenuNotFound_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException("Menu not found"));

        var act = () => _service.HandleAsync(nonExistentId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_PropagatesException()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var act = () => _service.HandleAsync(id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidId_ReturnsOkResult()
    {
        var menuId = Guid.NewGuid();
        var serviceMock = new Mock<GetMenu.IService>();
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

        var result = await GetMenu.Handler(serviceMock.Object, menuId);

        result.Should().BeOfType<Ok<MenuResponse>>();
        var okResult = (Ok<MenuResponse>)result;
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithId()
    {
        var menuId = Guid.NewGuid();
        var serviceMock = new Mock<GetMenu.IService>();
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

        await GetMenu.Handler(serviceMock.Object, menuId);

        serviceMock.Verify(s => s.HandleAsync(menuId), Times.Once);
    }

    #endregion
}
