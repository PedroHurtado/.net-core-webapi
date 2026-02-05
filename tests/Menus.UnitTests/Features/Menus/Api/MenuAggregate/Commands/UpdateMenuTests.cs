namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class UpdateMenuTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuAgg.Update _updateMenu;
    private readonly Mock<UpdateMenu.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateMenu.Service _service;

    public UpdateMenuTests()
    {
        _updateMenu = new(_menuValidator);
        _repositoryMock = new Mock<UpdateMenu.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UpdateMenu.Service(_updateMenu, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private MenuAgg CreateMenu(Guid? id = null)
    {
        var createCommand = new MenuAgg.Create(_menuValidator);
        return createCommand.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Original Menu",
            Description: "Original description"
        ));
    }

    private static UpdateMenu.Request CreateValidRequest(
        string name = "Updated Menu",
        string? description = null,
        DateTime? effectiveFrom = null,
        DateTime? effectiveUntil = null,
        int displayOrder = 0)
    {
        return new UpdateMenu.Request(
            Name: name,
            Description: description,
            EffectiveFrom: effectiveFrom,
            EffectiveUntil: effectiveUntil,
            DisplayOrder: displayOrder
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesMenu()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(name: "New Name");

        await _service.HandleAsync(menuId, request);

        menu.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_UpdatesAllProperties()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        var effectiveFrom = DateTime.UtcNow;
        var effectiveUntil = DateTime.UtcNow.AddMonths(3);
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            name: "Menú Actualizado",
            description: "Nueva descripción",
            effectiveFrom: effectiveFrom,
            effectiveUntil: effectiveUntil,
            displayOrder: 5
        );

        await _service.HandleAsync(menuId, request);

        menu.Name.Should().Be("Menú Actualizado");
        menu.Description.Should().Be("Nueva descripción");
        menu.EffectiveFrom.Should().Be(effectiveFrom);
        menu.EffectiveUntil.Should().Be(effectiveUntil);
        menu.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_PreservesIsActiveStatus()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        var originalIsActive = menu.IsActive;
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest();

        await _service.HandleAsync(menuId, request);

        menu.IsActive.Should().Be(originalIsActive);
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
    public async Task HandleAsync_WithEffectiveFromAfterEffectiveUntil_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            effectiveFrom: DateTime.UtcNow.AddDays(10),
            effectiveUntil: DateTime.UtcNow
        );

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
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var menuId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateMenu.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuId, request)).Returns(Task.CompletedTask);

        var result = await UpdateMenu.Handler(serviceMock.Object, menuId, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var menuId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateMenu.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<UpdateMenu.Request>())).Returns(Task.CompletedTask);

        await UpdateMenu.Handler(serviceMock.Object, menuId, request);

        serviceMock.Verify(s => s.HandleAsync(menuId, request), Times.Once);
    }

    #endregion
}
