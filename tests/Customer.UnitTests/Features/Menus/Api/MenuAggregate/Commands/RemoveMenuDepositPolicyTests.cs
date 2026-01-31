namespace Customer.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class RemoveMenuDepositPolicyTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly DepositPolicyValidator _depositPolicyValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.SetDepositPolicy _setDepositPolicy;
    private readonly MenuAgg.RemoveDepositPolicy _removeDepositPolicy;
    private readonly Mock<RemoveMenuDepositPolicy.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveMenuDepositPolicy.Service _service;

    public RemoveMenuDepositPolicyTests()
    {
        var createDepositPolicy = new DepositPolicyVO.Create(_depositPolicyValidator);
        _createMenu = new(_menuValidator);
        _setDepositPolicy = new(_menuValidator, createDepositPolicy);
        _removeDepositPolicy = new(_menuValidator);

        _repositoryMock = new Mock<RemoveMenuDepositPolicy.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new RemoveMenuDepositPolicy.Service(_removeDepositPolicy, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private MenuAgg CreateMenuWithDepositPolicy()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Test Menu"
        ));
        _setDepositPolicy.Execute(menu, new SetDepositPolicyCommand(DepositType.FixedAmount, 100.00m));
        return menu;
    }

    private MenuAgg CreateMenuWithoutDepositPolicy()
    {
        return _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Test Menu"
        ));
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithMenuHavingDepositPolicy_RemovesDepositPolicy()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithDepositPolicy();
        menu.DepositPolicy.Should().NotBeNull();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        await _service.HandleAsync(menuId);

        menu.DepositPolicy.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithMenuWithoutDepositPolicy_Succeeds()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithoutDepositPolicy();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        await _service.HandleAsync(menuId);

        menu.DepositPolicy.Should().BeNull();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithDepositPolicy();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        await _service.HandleAsync(menuId);

        _repositoryMock.Verify(r => r.Get(menuId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenuWithDepositPolicy();
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

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var menuId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveMenuDepositPolicy.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuId)).Returns(Task.CompletedTask);

        var result = await RemoveMenuDepositPolicy.Handler(serviceMock.Object, menuId);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectId()
    {
        var menuId = Guid.NewGuid();
        var serviceMock = new Mock<RemoveMenuDepositPolicy.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await RemoveMenuDepositPolicy.Handler(serviceMock.Object, menuId);

        serviceMock.Verify(s => s.HandleAsync(menuId), Times.Once);
    }

    #endregion
}
