namespace Customer.UnitTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemAllergenTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly AllergenValidator _allergenValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly Allergen.Create _allergenCreate;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.AddAllergen _addAllergen;
    private readonly MenuItemAgg.RemoveAllergen _removeAllergen;
    private readonly Mock<RemoveMenuItemAllergen.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveMenuItemAllergen.Service _service;

    public RemoveMenuItemAllergenTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _allergenCreate = new(_allergenValidator);
        _createMenuItem = new(_priceOptionCreate, _menuItemValidator);
        _addAllergen = new(_menuItemValidator);
        _removeAllergen = new(_menuItemValidator);
        _repositoryMock = new Mock<RemoveMenuItemAllergen.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new RemoveMenuItemAllergen.Service(
            _removeAllergen,
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static CreatePriceOptionCommand[] CreateValidPriceOptionCommands() =>
    [
        new CreatePriceOptionCommand(PortionType.Full, 22.00m)
    ];

    private MenuItemAgg CreateMenuItem(string name = "Test MenuItem")
    {
        var command = new CreateMenuItemCommand(
            TenantId: _tenantId,
            Name: name,
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: CreateValidPriceOptionCommands()
        );

        return _createMenuItem.Execute(command);
    }

    private Allergen CreateAllergen(string code = "GLUTEN", string name = "Gluten")
    {
        var command = new CreateAllergenCommand(
            Code: code,
            Name: name,
            IconUrl: null,
            IsActive: true,
            DisplayOrder: 0
        );
        return _allergenCreate.Execute(command);
    }

    private MenuItemAgg CreateMenuItemWithAllergen(string allergenCode = "GLUTEN", string allergenName = "Gluten")
    {
        var menuItem = CreateMenuItem();
        var allergen = CreateAllergen(allergenCode, allergenName);
        return _addAllergen.Execute(menuItem, new AddAllergenCommand(allergen));
    }

    private void SetupRepositoryGet(MenuItemAgg menuItem)
    {
        _repositoryMock.Setup(r => r.Get(menuItem.Id)).ReturnsAsync(menuItem);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithExistingAllergen_RemovesAllergen()
    {
        // Arrange
        var menuItem = CreateMenuItemWithAllergen("GLUTEN", "Gluten");
        SetupRepositoryGet(menuItem);

        // Act
        await _service.HandleAsync(menuItem.Id, "GLUTEN");

        // Assert
        menuItem.Allergens.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithMultipleAllergens_RemovesOnlySpecified()
    {
        // Arrange
        var menuItem = CreateMenuItemWithAllergen("GLUTEN", "Gluten");
        var lactoseAllergen = CreateAllergen("LACTOSE", "Lactosa");
        menuItem = _addAllergen.Execute(menuItem, new AddAllergenCommand(lactoseAllergen));
        SetupRepositoryGet(menuItem);

        // Act
        await _service.HandleAsync(menuItem.Id, "GLUTEN");

        // Assert
        menuItem.Allergens.Should().ContainSingle();
        menuItem.Allergens.Should().Contain(a => a.Id == "LACTOSE");
        menuItem.Allergens.Should().NotContain(a => a.Id == "GLUTEN");
    }

    [Fact]
    public async Task HandleAsync_PreservesOtherProperties()
    {
        // Arrange
        var createCommand = new CreateMenuItemCommand(
            TenantId: _tenantId,
            Name: "Test MenuItem",
            Description: "Test Description",
            ImageUrl: "https://example.com/image.jpg",
            DisplayOrder: 5,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: CreateValidPriceOptionCommands()
        );
        var menuItem = _createMenuItem.Execute(createCommand);
        var allergen = CreateAllergen();
        menuItem = _addAllergen.Execute(menuItem, new AddAllergenCommand(allergen));
        SetupRepositoryGet(menuItem);

        // Act
        await _service.HandleAsync(menuItem.Id, "GLUTEN");

        // Assert
        menuItem.Name.Should().Be("Test MenuItem");
        menuItem.Description.Should().Be("Test Description");
        menuItem.ImageUrl.Should().Be("https://example.com/image.jpg");
        menuItem.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_PreservesExistingPriceOptions()
    {
        // Arrange
        var menuItem = CreateMenuItemWithAllergen();
        var existingOption = menuItem.PriceOptions.First();
        SetupRepositoryGet(menuItem);

        // Act
        await _service.HandleAsync(menuItem.Id, "GLUTEN");

        // Assert
        menuItem.PriceOptions.Should().Contain(p =>
            p.PortionType == existingOption.PortionType &&
            p.Price == existingOption.Price);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        // Arrange
        var menuItem = CreateMenuItemWithAllergen();
        SetupRepositoryGet(menuItem);

        // Act
        await _service.HandleAsync(menuItem.Id, "GLUTEN");

        // Assert
        _repositoryMock.Verify(r => r.Get(menuItem.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        // Arrange
        var menuItem = CreateMenuItemWithAllergen();
        SetupRepositoryGet(menuItem);

        // Act
        await _service.HandleAsync(menuItem.Id, "GLUTEN");

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetsMenuItemBeforeSaving()
    {
        // Arrange
        var menuItem = CreateMenuItemWithAllergen();
        var callOrder = new List<string>();
        _repositoryMock.Setup(r => r.Get(menuItem.Id))
            .Callback(() => callOrder.Add("Get"))
            .ReturnsAsync(menuItem);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        // Act
        await _service.HandleAsync(menuItem.Id, "GLUTEN");

        // Assert
        callOrder.Should().ContainInOrder("Get", "SaveChanges");
    }

    #endregion

    #region NotFound Tests

    [Fact]
    public async Task HandleAsync_WhenMenuItemNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException("MenuItem not found"));

        // Act
        var act = () => _service.HandleAsync(nonExistentId, "GLUTEN");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenAllergenNotInMenuItem_ThrowsKeyNotFoundException()
    {
        // Arrange
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        // Act
        var act = () => _service.HandleAsync(menuItem.Id, "SULFITOS");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Allergen not found in this item");
    }

    [Fact]
    public async Task HandleAsync_WhenAllergenNotFound_DoesNotCallSaveChanges()
    {
        // Arrange
        var menuItem = CreateMenuItem();
        SetupRepositoryGet(menuItem);

        // Act
        try { await _service.HandleAsync(menuItem.Id, "SULFITOS"); } catch { }

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var act = () => _service.HandleAsync(id, "GLUTEN");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var allergenId = "GLUTEN";
        var serviceMock = new Mock<RemoveMenuItemAllergen.IService>();
        serviceMock.Setup(s => s.HandleAsync(menuItemId, allergenId)).Returns(Task.CompletedTask);

        // Act
        var result = await RemoveMenuItemAllergen.Handler(serviceMock.Object, menuItemId, allergenId);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithIdAndAllergenId()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var allergenId = "GLUTEN";
        var serviceMock = new Mock<RemoveMenuItemAllergen.IService>();
        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        await RemoveMenuItemAllergen.Handler(serviceMock.Object, menuItemId, allergenId);

        // Assert
        serviceMock.Verify(s => s.HandleAsync(menuItemId, allergenId), Times.Once);
    }

    #endregion
}
