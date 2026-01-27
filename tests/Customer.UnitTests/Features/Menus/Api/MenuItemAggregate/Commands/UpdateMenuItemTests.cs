namespace Customer.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class UpdateMenuItemTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.Update _updateMenuItem;
    private readonly Mock<UpdateMenuItem.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateMenuItem.Service _service;

    public UpdateMenuItemTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _updateMenuItem = new(_menuItemValidator);
        _repositoryMock = new Mock<UpdateMenuItem.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UpdateMenuItem.Service(_updateMenuItem, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private MenuItemAgg CreateExistingMenuItem(
        string name = "Original Name",
        string? description = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isHighRiskItem = false,
        bool requiresAdvanceOrder = false,
        int? minimumAdvanceOrderQuantity = null,
        bool isAlwaysAvailable = true,
        DayOfWeek[]? availableDays = null,
        string? allergenNotes = null)
    {
        var command = new CreateMenuItemCommand(
            TenantId: _tenantId,
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
            PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, 22.00m)]);

        return _createMenuItem.Execute(command);
    }

    private void SetupRepositoryGet(MenuItemAgg menuItem)
    {
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);
    }

    private static UpdateMenuItem.Request CreateValidRequest(
        string name = "Updated Name",
        string? description = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isHighRiskItem = false,
        bool requiresAdvanceOrder = false,
        int? minimumAdvanceOrderQuantity = null,
        bool isAlwaysAvailable = true,
        DayOfWeek[]? availableDays = null,
        string? allergenNotes = null)
    {
        return new UpdateMenuItem.Request(
            Name: name,
            Description: description,
            ImageUrl: imageUrl,
            DisplayOrder: displayOrder,
            IsHighRiskItem: isHighRiskItem,
            RequiresAdvanceOrder: requiresAdvanceOrder,
            MinimumAdvanceOrderQuantity: minimumAdvanceOrderQuantity,
            IsAlwaysAvailable: isAlwaysAvailable,
            AvailableDays: availableDays ?? [],
            AllergenNotes: allergenNotes
        );
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesMenuItem()
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(name: "Pulpo a la Gallega");

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.Name.Should().Be("Pulpo a la Gallega");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_UpdatesAllFields()
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(
            name: "Updated Dish",
            description: "Updated Description",
            imageUrl: "https://example.com/updated.jpg",
            displayOrder: 10,
            isHighRiskItem: true,
            requiresAdvanceOrder: true,
            minimumAdvanceOrderQuantity: 4,
            isAlwaysAvailable: false,
            availableDays: [DayOfWeek.Friday, DayOfWeek.Saturday],
            allergenNotes: "Contains gluten"
        );

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.Name.Should().Be("Updated Dish");
        menuItem.Description.Should().Be("Updated Description");
        menuItem.ImageUrl.Should().Be("https://example.com/updated.jpg");
        menuItem.DisplayOrder.Should().Be(10);
        menuItem.IsHighRiskItem.Should().BeTrue();
        menuItem.RequiresAdvanceOrder.Should().BeTrue();
        menuItem.MinimumAdvanceOrderQuantity.Should().Be(4);
        menuItem.IsAlwaysAvailable.Should().BeFalse();
        menuItem.AvailableDays.Should().BeEquivalentTo([DayOfWeek.Friday, DayOfWeek.Saturday]);
        menuItem.AllergenNotes.Should().Be("Contains gluten");
    }

    [Fact]
    public async Task HandleAsync_WithNullOptionalFields_ClearsOptionalFields()
    {
        var menuItem = CreateExistingMenuItem(
            description: "Original description",
            imageUrl: "https://example.com/original.jpg",
            allergenNotes: "Original notes"
        );
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(
            description: null,
            imageUrl: null,
            allergenNotes: null
        );

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.Description.Should().BeNull();
        menuItem.ImageUrl.Should().BeNull();
        menuItem.AllergenNotes.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ChangingAvailability_UpdatesAvailableDays()
    {
        var menuItem = CreateExistingMenuItem(isAlwaysAvailable: true);
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(
            isAlwaysAvailable: false,
            availableDays: [DayOfWeek.Monday, DayOfWeek.Wednesday]
        );

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.IsAlwaysAvailable.Should().BeFalse();
        menuItem.AvailableDays.Should().BeEquivalentTo([DayOfWeek.Monday, DayOfWeek.Wednesday]);
    }

    [Fact]
    public async Task HandleAsync_PreservesMenuItemId()
    {
        var menuItem = CreateExistingMenuItem();
        var originalId = menuItem.Id;
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(name: "New Name");

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.Id.Should().Be(originalId);
    }

    [Fact]
    public async Task HandleAsync_PreservesTenantId()
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(name: "New Name");

        await _service.HandleAsync(menuItem.Id, request);

        menuItem.TenantId.Should().Be(_tenantId);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest();

        await _service.HandleAsync(menuItem.Id, request);

        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest();

        await _service.HandleAsync(menuItem.Id, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetsMenuItemBeforeSaving()
    {
        var menuItem = CreateExistingMenuItem();
        var callOrder = new List<string>();
        _repositoryMock.Setup(r => r.Get(menuItem.Id))
            .Callback(() => callOrder.Add("Get"))
            .ReturnsAsync(menuItem);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        var request = CreateValidRequest();

        await _service.HandleAsync(menuItem.Id, request);

        callOrder.Should().ContainInOrder("Get", "SaveChanges");
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException(string? name)
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(name: name!);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(name: new string('a', 101));

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidImageUrl_ThrowsValidationException()
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(imageUrl: "not-a-url");

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(displayOrder: -1);

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithRequiresAdvanceOrderButNotHighRisk_ThrowsValidationException()
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(
            isHighRiskItem: false,
            requiresAdvanceOrder: true
        );

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNotAlwaysAvailableWithoutDays_ThrowsValidationException()
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(
            isAlwaysAvailable: false,
            availableDays: []
        );

        var act = () => _service.HandleAsync(menuItem.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var menuItem = CreateExistingMenuItem();
        SetupRepositoryGet(menuItem);
        var request = CreateValidRequest(name: "");

        try { await _service.HandleAsync(menuItem.Id, request); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateMenuItem.IService>();

        serviceMock.Setup(s => s.HandleAsync(menuItemId, request)).Returns(Task.CompletedTask);

        var result = await UpdateMenuItem.Handler(serviceMock.Object, menuItemId, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithIdAndRequest()
    {
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateMenuItem.IService>();

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<UpdateMenuItem.Request>())).Returns(Task.CompletedTask);

        await UpdateMenuItem.Handler(serviceMock.Object, menuItemId, request);

        serviceMock.Verify(s => s.HandleAsync(menuItemId, request), Times.Once);
    }

    #endregion
}
