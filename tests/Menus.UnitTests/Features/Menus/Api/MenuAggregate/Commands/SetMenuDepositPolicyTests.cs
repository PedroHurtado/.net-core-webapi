namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class SetMenuDepositPolicyTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly DepositPolicyValidator _depositPolicyValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.SetDepositPolicy _setDepositPolicy;
    private readonly Mock<SetMenuDepositPolicy.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SetMenuDepositPolicy.Service _service;

    public SetMenuDepositPolicyTests()
    {
        var createDepositPolicy = new DepositPolicyVO.Create(_depositPolicyValidator);
        _createMenu = new(_menuValidator);
        _setDepositPolicy = new(_menuValidator, createDepositPolicy);

        _repositoryMock = new Mock<SetMenuDepositPolicy.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new SetMenuDepositPolicy.Service(_setDepositPolicy, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private MenuAgg CreateMenu()
    {
        return _createMenu.Execute(new CreateMenuCommand(
            TenantId: _tenantId,
            Name: "Test Menu"
        ));
    }

    private static SetMenuDepositPolicy.Request CreateValidRequest(
        DepositType depositType = DepositType.FixedAmount,
        decimal amount = 50.00m,
        decimal? percentage = null,
        decimal? minimumBillForDeposit = null,
        int? minimumGuestsForDeposit = null)
    {
        return new SetMenuDepositPolicy.Request(
            DepositType: depositType,
            Amount: amount,
            Percentage: percentage,
            MinimumBillForDeposit: minimumBillForDeposit,
            MinimumGuestsForDeposit: minimumGuestsForDeposit
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithFixedAmount_SetsDepositPolicy()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            depositType: DepositType.FixedAmount,
            amount: 100.00m
        );

        await _service.HandleAsync(menuId, request);

        menu.DepositPolicy.Should().NotBeNull();
        menu.DepositPolicy!.DepositType.Should().Be(DepositType.FixedAmount);
        menu.DepositPolicy.Amount.Should().Be(100.00m);
    }

    [Fact]
    public async Task HandleAsync_WithPerPerson_SetsDepositPolicy()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            depositType: DepositType.PerPerson,
            amount: 25.00m
        );

        await _service.HandleAsync(menuId, request);

        menu.DepositPolicy.Should().NotBeNull();
        menu.DepositPolicy!.DepositType.Should().Be(DepositType.PerPerson);
        menu.DepositPolicy.Amount.Should().Be(25.00m);
    }

    [Fact]
    public async Task HandleAsync_WithPercentageOfBill_SetsDepositPolicy()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            depositType: DepositType.PercentageOfBill,
            amount: 10.00m,
            percentage: 20m
        );

        await _service.HandleAsync(menuId, request);

        menu.DepositPolicy.Should().NotBeNull();
        menu.DepositPolicy!.DepositType.Should().Be(DepositType.PercentageOfBill);
        menu.DepositPolicy.Percentage.Should().Be(20m);
    }

    [Fact]
    public async Task HandleAsync_WithThresholds_SetsDepositPolicyWithThresholds()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            depositType: DepositType.FixedAmount,
            amount: 50.00m,
            minimumBillForDeposit: 200.00m,
            minimumGuestsForDeposit: 6
        );

        await _service.HandleAsync(menuId, request);

        menu.DepositPolicy.Should().NotBeNull();
        menu.DepositPolicy!.MinimumBillForDeposit.Should().Be(200.00m);
        menu.DepositPolicy.MinimumGuestsForDeposit.Should().Be(6);
    }

    [Fact]
    public async Task HandleAsync_ReplacesExistingDepositPolicy()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _setDepositPolicy.Execute(menu, new SetDepositPolicyCommand(DepositType.FixedAmount, 100m));
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            depositType: DepositType.PerPerson,
            amount: 25.00m
        );

        await _service.HandleAsync(menuId, request);

        menu.DepositPolicy!.DepositType.Should().Be(DepositType.PerPerson);
        menu.DepositPolicy.Amount.Should().Be(25.00m);
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

    [Fact]
    public async Task HandleAsync_WithZeroAmount_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(amount: 0);

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativeAmount_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(amount: -10.00m);

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithPercentageOfBillWithoutPercentage_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            depositType: DepositType.PercentageOfBill,
            percentage: null
        );

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithPercentageOver100_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            depositType: DepositType.PercentageOfBill,
            percentage: 101m
        );

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithPercentageUnder1_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(
            depositType: DepositType.PercentageOfBill,
            percentage: 0m
        );

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithMinimumGuestsUnder1_ThrowsValidationException()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(minimumGuestsForDeposit: 0);

        var act = () => _service.HandleAsync(menuId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var menuId = Guid.NewGuid();
        var menu = CreateMenu();
        _repositoryMock.Setup(r => r.Get(menuId)).ReturnsAsync(menu);

        var request = CreateValidRequest(amount: 0);

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
        var serviceMock = new Mock<SetMenuDepositPolicy.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuId, request)).Returns(Task.CompletedTask);

        var result = await SetMenuDepositPolicy.Handler(serviceMock.Object, menuId, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var menuId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<SetMenuDepositPolicy.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<SetMenuDepositPolicy.Request>())).Returns(Task.CompletedTask);

        await SetMenuDepositPolicy.Handler(serviceMock.Object, menuId, request);

        serviceMock.Verify(s => s.HandleAsync(menuId, request), Times.Once);
    }

    #endregion
}
