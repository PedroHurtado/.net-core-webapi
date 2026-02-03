namespace Customer.UnitTests.Features.Menus.Api.MenuAggregate.Commands;

public class CreateMenuTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly Mock<CreateMenu.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateMenu.Service _service;

    public CreateMenuTests()
    {
        _createMenu = new(_menuValidator);
        _repositoryMock = new Mock<CreateMenu.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new CreateMenu.Service(_tenantId, _createMenu, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static CreateMenu.Request CreateValidRequest(
        string name = "Menú Degustación",
        string? description = null,
        DateTime? effectiveFrom = null,
        DateTime? effectiveUntil = null)
    {
        return new CreateMenu.Request(
            Name: name,
            Description: description,
            EffectiveFrom: effectiveFrom,
            EffectiveUntil: effectiveUntil
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsResponse()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.Should().NotBeNull();
        response.Name.Should().Be("Menú Degustación");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_ReturnsCompleteResponse()
    {
        var effectiveFrom = DateTime.UtcNow;
        var effectiveUntil = DateTime.UtcNow.AddMonths(3);
        var request = CreateValidRequest(
            name: "Menú Navideño",
            description: "Menú especial para las fiestas",
            effectiveFrom: effectiveFrom,
            effectiveUntil: effectiveUntil
        );

        var response = await _service.HandleAsync(request);

        response.Name.Should().Be("Menú Navideño");
        response.Description.Should().Be("Menú especial para las fiestas");
        response.EffectiveFrom.Should().Be(effectiveFrom);
        response.EffectiveUntil.Should().Be(effectiveUntil);
    }

    [Fact]
    public async Task HandleAsync_CreatesMenuAsInactive()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_CreatesMenuWithDisplayOrderZero()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_CreatesMenuWithEmptyCategories()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.Categories.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_CreatesMenuWithoutDepositPolicy()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.DepositPolicy.Should().BeNull();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryAdd()
    {
        var request = CreateValidRequest();

        await _service.HandleAsync(request);

        _repositoryMock.Verify(r => r.Add(It.Is<MenuAgg>(m => m.Name == "Menú Degustación")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var request = CreateValidRequest();

        await _service.HandleAsync(request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AddsMenuBeforeSaving()
    {
        var callOrder = new List<string>();
        _repositoryMock.Setup(r => r.Add(It.IsAny<MenuAgg>())).Callback(() => callOrder.Add("Add"));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        var request = CreateValidRequest();

        await _service.HandleAsync(request);

        callOrder.Should().ContainInOrder("Add", "SaveChanges");
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException(string name)
    {
        var request = CreateValidRequest(name: name);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNameExceeding100Characters_ThrowsValidationException()
    {
        var request = CreateValidRequest(name: new string('a', 101));

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithDescriptionExceeding500Characters_ThrowsValidationException()
    {
        var request = CreateValidRequest(description: new string('a', 501));

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithEffectiveFromAfterEffectiveUntil_ThrowsValidationException()
    {
        var request = CreateValidRequest(
            effectiveFrom: DateTime.UtcNow.AddDays(10),
            effectiveUntil: DateTime.UtcNow
        );

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallRepository()
    {
        var request = CreateValidRequest(name: "");

        try { await _service.HandleAsync(request); } catch { }

        _repositoryMock.Verify(r => r.Add(It.IsAny<MenuAgg>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var request = CreateValidRequest(name: "");

        try { await _service.HandleAsync(request); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsCreatedResult()
    {
        var request = CreateValidRequest();
        var serviceMock = new Mock<CreateMenu.IService>();
        var expectedResponse = new MenuResponse(
            Id: Guid.NewGuid(),
            Name: "Menú Degustación",
            Description: null,
            IsActive: false,
            DisplayOrder: 0,
            EffectiveFrom: null,
            EffectiveUntil: null,
            DepositPolicy: null,
            Categories: []
        );

        serviceMock.Setup(s => s.HandleAsync(request)).ReturnsAsync(expectedResponse);

        var result = await CreateMenu.Handler(serviceMock.Object, request);

        result.Should().BeOfType<Created<MenuResponse>>();
        var createdResult = (Created<MenuResponse>)result;
        createdResult.Location.Should().Be($"/menus/{expectedResponse.Id}");
        createdResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithRequest()
    {
        var request = CreateValidRequest();
        var serviceMock = new Mock<CreateMenu.IService>();
        var expectedResponse = new MenuResponse(
            Id: Guid.NewGuid(),
            Name: "Test",
            Description: null,
            IsActive: false,
            DisplayOrder: 0,
            EffectiveFrom: null,
            EffectiveUntil: null,
            DepositPolicy: null,
            Categories: []
        );

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<CreateMenu.Request>())).ReturnsAsync(expectedResponse);

        await CreateMenu.Handler(serviceMock.Object, request);

        serviceMock.Verify(s => s.HandleAsync(request), Times.Once);
    }

    #endregion
}
