namespace Customer.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class DeactivateMenuTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.Deactivate _deactivateMenu;
    private readonly Mock<DeactivateMenu.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateMenu.Service _service;

    public DeactivateMenuTests()
    {
        _createMenu = new(_menuValidator);
        _deactivateMenu = new(_menuValidator);

        _repositoryMock = new Mock<DeactivateMenu.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new DeactivateMenu.Service(_deactivateMenu, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private MenuAgg CreateActiveMenu()
    {
        return _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Active Menu"
        ));
    }

    private MenuAgg CreateInactiveMenu()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Inactive Menu"
        ));
        _deactivateMenu.Execute(menu);
        return menu;
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
            TenantId: _tenantId,
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
            TenantId: _tenantId,
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
