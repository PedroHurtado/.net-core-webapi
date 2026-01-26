namespace Customer.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class CreateMenuItemTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly Mock<CreateMenuItem.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateMenuItem.Service _service;

    public CreateMenuItemTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _repositoryMock = new Mock<CreateMenuItem.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new CreateMenuItem.Service(_tenantId, _createMenuItem, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static CreateMenuItem.Request CreateValidRequest(
        string name = "Pulpo al Horno",
        string? description = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isHighRiskItem = false,
        bool requiresAdvanceOrder = false,
        int? minimumAdvanceOrderQuantity = null,
        bool isAlwaysAvailable = true,
        DayOfWeek[]? availableDays = null,
        string? allergenNotes = null,
        CreateMenuItem.CreatePriceOptionRequest[]? priceOptions = null)
    {
        return new CreateMenuItem.Request(
            Name: name,
            Description: description,
            ImageUrl: imageUrl,
            DisplayOrder: displayOrder,
            IsHighRiskItem: isHighRiskItem,
            RequiresAdvanceOrder: requiresAdvanceOrder,
            MinimumAdvanceOrderQuantity: minimumAdvanceOrderQuantity,
            IsAlwaysAvailable: isAlwaysAvailable,
            AvailableDays: availableDays ?? [],
            AllergenNotes: allergenNotes,
            PriceOptions: priceOptions ?? [new(PortionType.Full, 22.00m)]
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsResponse()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.Should().NotBeNull();
        response.Name.Should().Be("Pulpo al Horno");
        response.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_ReturnsCompleteResponse()
    {
        var request = CreateValidRequest(
            name: "Jamón Ibérico",
            description: "Jamón de bellota",
            imageUrl: "https://example.com/jamon.jpg",
            displayOrder: 5,
            isHighRiskItem: true,
            requiresAdvanceOrder: true,
            minimumAdvanceOrderQuantity: 4,
            isAlwaysAvailable: false,
            availableDays: [DayOfWeek.Friday, DayOfWeek.Saturday],
            allergenNotes: "Puede contener trazas",
            priceOptions: [
                new(PortionType.Small, 3.50m),
                new(PortionType.Half, 7.00m),
                new(PortionType.Full, 14.00m)
            ]
        );

        var response = await _service.HandleAsync(request);

        response.Name.Should().Be("Jamón Ibérico");
        response.Description.Should().Be("Jamón de bellota");
        response.ImageUrl.Should().Be("https://example.com/jamon.jpg");
        response.DisplayOrder.Should().Be(5);
        response.IsHighRiskItem.Should().BeTrue();
        response.RequiresAdvanceOrder.Should().BeTrue();
        response.MinimumAdvanceOrderQuantity.Should().Be(4);
        response.IsAlwaysAvailable.Should().BeFalse();
        response.AvailableDays.Should().Contain(DayOfWeek.Friday);
        response.AvailableDays.Should().Contain(DayOfWeek.Saturday);
        response.AllergenNotes.Should().Be("Puede contener trazas");
        response.PriceOptions.Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_CreatesMenuItemAsInactive()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_CreatesMenuItemAsAvailable()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithMultiplePriceOptions_MapsAllOptions()
    {
        var request = CreateValidRequest(
            priceOptions: [
                new(PortionType.Small, 3.50m),
                new(PortionType.Half, 7.00m),
                new(PortionType.Full, 14.00m)
            ]
        );

        var response = await _service.HandleAsync(request);

        response.PriceOptions.Should().HaveCount(3);
        response.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Small && p.Price == 3.50m);
        response.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.Price == 7.00m);
        response.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full && p.Price == 14.00m);
    }

    [Fact]
    public async Task HandleAsync_WithMarketPriceOption_MapsCorrectly()
    {
        var request = CreateValidRequest(
            priceOptions: [new(PortionType.MarketPrice, null)]
        );

        var response = await _service.HandleAsync(request);

        response.PriceOptions.Should().HaveCount(1);
        var priceOption = response.PriceOptions.First();
        priceOption.PortionType.Should().Be(PortionType.MarketPrice);
        priceOption.Price.Should().BeNull();
        priceOption.RequiresMarketPrice.Should().BeTrue();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryAdd()
    {
        var request = CreateValidRequest();

        await _service.HandleAsync(request);

        _repositoryMock.Verify(r => r.Add(It.Is<MenuItemAgg>(m => m.Name == "Pulpo al Horno")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var request = CreateValidRequest();

        await _service.HandleAsync(request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AddsMenuItemBeforeSaving()
    {
        var callOrder = new List<string>();
        _repositoryMock.Setup(r => r.Add(It.IsAny<MenuItemAgg>())).Callback(() => callOrder.Add("Add"));
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
    public async Task HandleAsync_WithInvalidImageUrl_ThrowsValidationException()
    {
        var request = CreateValidRequest(imageUrl: "not-a-url");

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNoPriceOptions_ThrowsValidationException()
    {
        var request = CreateValidRequest(priceOptions: []);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithDuplicatePortionTypes_ThrowsValidationException()
    {
        var request = CreateValidRequest(
            priceOptions: [
                new(PortionType.Full, 10.00m),
                new(PortionType.Full, 12.00m)
            ]
        );

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithRequiresAdvanceOrderWithoutHighRisk_ThrowsValidationException()
    {
        var request = CreateValidRequest(
            isHighRiskItem: false,
            requiresAdvanceOrder: true
        );

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNotAlwaysAvailableWithoutDays_ThrowsValidationException()
    {
        var request = CreateValidRequest(
            isAlwaysAvailable: false,
            availableDays: []
        );

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativePrice_ThrowsValidationException()
    {
        var request = CreateValidRequest(
            priceOptions: [new(PortionType.Full, -10.00m)]
        );

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithFixedPortionWithoutPrice_ThrowsValidationException()
    {
        var request = CreateValidRequest(
            priceOptions: [new(PortionType.Full, null)]
        );

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallRepository()
    {
        var request = CreateValidRequest(name: "");

        try { await _service.HandleAsync(request); } catch { }

        _repositoryMock.Verify(r => r.Add(It.IsAny<MenuItemAgg>()), Times.Never);
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
        var serviceMock = new Mock<CreateMenuItem.IService>();
        var expectedResponse = new MenuItemResponse(
            Id: Guid.NewGuid(),
            TenantId: _tenantId,
            Name: "Pulpo al Horno",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsActive: false,
            IsAvailable: true,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AllergenNotes: null,
            IsAvailableToday: true,
            CanBeOrdered: false,
            HasDepositOverride: false,
            DepositOverride: null,
            NutritionalInfo: null,
            PriceOptions: [new PriceOptionResponse(PortionType.Full, 22.00m, true, false, "22,00 €")],
            AvailableDays: [],
            Allergens: []
        );

        serviceMock.Setup(s => s.HandleAsync(request)).ReturnsAsync(expectedResponse);

        var result = await CreateMenuItem.Handler(serviceMock.Object, request);

        result.Should().BeOfType<Created<MenuItemResponse>>();
        var createdResult = (Created<MenuItemResponse>)result;
        createdResult.Location.Should().Be($"/menu-items/{expectedResponse.Id}");
        createdResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithRequest()
    {
        var request = CreateValidRequest();
        var serviceMock = new Mock<CreateMenuItem.IService>();
        var expectedResponse = new MenuItemResponse(
            Id: Guid.NewGuid(),
            TenantId: _tenantId,
            Name: "Test",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsActive: false,
            IsAvailable: true,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AllergenNotes: null,
            IsAvailableToday: true,
            CanBeOrdered: false,
            HasDepositOverride: false,
            DepositOverride: null,
            NutritionalInfo: null,
            PriceOptions: [],
            AvailableDays: [],
            Allergens: []
        );

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<CreateMenuItem.Request>())).ReturnsAsync(expectedResponse);

        await CreateMenuItem.Handler(serviceMock.Object, request);

        serviceMock.Verify(s => s.HandleAsync(request), Times.Once);
    }

    #endregion
}
