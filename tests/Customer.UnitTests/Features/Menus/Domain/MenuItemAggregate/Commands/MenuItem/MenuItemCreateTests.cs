namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemCreateTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly AllergenValidator _allergenValidator = new();
    private readonly MenuItemAgg.Create _create;
    private readonly PriceOptionVO.Create _createPriceOption;
    private readonly AllergenAgg.Create _createAllergen;

    public MenuItemCreateTests()
    {
        _create = new(_validator);
        _createPriceOption = new(_priceOptionValidator);
        _createAllergen = new(_allergenValidator);
    }

    private HashSet<PriceOption> CreateValidPriceOptions() =>
    [
        _createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 10.99m))
    ];

    [Fact]
    public void Execute_WithValidCommand_ReturnsMenuItem()
    {
        var tenantId = Guid.NewGuid();
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(tenantId, "Caesar Salad", priceOptions);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.TenantId.Should().Be(tenantId);
        result.Name.Should().Be("Caesar Salad");
        result.Description.Should().BeNull();
        result.ImageUrl.Should().BeNull();
        result.DisplayOrder.Should().Be(0);
        result.IsActive.Should().BeTrue();
        result.IsAvailable.Should().BeTrue();
        result.IsHighRiskItem.Should().BeFalse();
        result.RequiresAdvanceOrder.Should().BeFalse();
        result.MinimumAdvanceOrderQuantity.Should().BeNull();
        result.IsAlwaysAvailable.Should().BeTrue();
        result.PriceOptions.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_WithOptionalFields_SetsOptionalFields()
    {
        var tenantId = Guid.NewGuid();
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(
            TenantId: tenantId,
            Name: "Wagyu Steak",
            PriceOptions: priceOptions,
            Description: "Premium Japanese beef",
            ImageUrl: "https://example.com/wagyu.jpg",
            DisplayOrder: 5,
            IsHighRiskItem: true,
            RequiresAdvanceOrder: true,
            MinimumAdvanceOrderQuantity: 10,
            AllergenNotes: "May contain traces of soy");

        var result = _create.Execute(command);

        result.Description.Should().Be("Premium Japanese beef");
        result.ImageUrl.Should().Be("https://example.com/wagyu.jpg");
        result.DisplayOrder.Should().Be(5);
        result.IsHighRiskItem.Should().BeTrue();
        result.RequiresAdvanceOrder.Should().BeTrue();
        result.MinimumAdvanceOrderQuantity.Should().Be(10);
        result.AllergenNotes.Should().Be("May contain traces of soy");
    }

    [Fact]
    public void Execute_WithMultiplePriceOptions_SetsAllPriceOptions()
    {
        var priceOptions = new HashSet<PriceOption>
        {
            _createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 15.99m)),
            _createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Half, 9.99m))
        };
        var command = new CreateMenuItemCommand(Guid.NewGuid(), "Pasta", priceOptions);

        var result = _create.Execute(command);

        result.PriceOptions.Should().HaveCount(2);
    }

    [Fact]
    public void Execute_WithAvailableDays_SetsAvailableDays()
    {
        var priceOptions = CreateValidPriceOptions();
        var availableDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
        var command = new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
            Name: "Weekly Special",
            PriceOptions: priceOptions,
            IsAlwaysAvailable: false,
            AvailableDays: availableDays);

        var result = _create.Execute(command);

        result.IsAlwaysAvailable.Should().BeFalse();
        result.AvailableDays.Should().BeEquivalentTo(availableDays);
    }

    [Fact]
    public void Execute_WithAllergens_SetsAllergens()
    {
        var priceOptions = CreateValidPriceOptions();
        var allergens = new HashSet<Allergen>
        {
            _createAllergen.Execute(new CreateAllergenCommand("GLUTEN", "Gluten")),
            _createAllergen.Execute(new CreateAllergenCommand("LACTEOS", "Lácteos"))
        };
        var command = new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
            Name: "Pizza Margherita",
            PriceOptions: priceOptions,
            Allergens: allergens);

        var result = _create.Execute(command);

        result.Allergens.Should().HaveCount(2);
        result.Allergens.Should().BeEquivalentTo(allergens);
    }

    #region Validation Throws

    [Fact]
    public void Execute_WithEmptyTenantId_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(Guid.Empty, "Item", priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(Guid.NewGuid(), name!, priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(Guid.NewGuid(), new string('a', 151), priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            priceOptions,
            Description: new string('a', 1001));

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithEmptyPriceOptions_ThrowsValidationException()
    {
        var command = new CreateMenuItemCommand(Guid.NewGuid(), "Item", []);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            priceOptions,
            DisplayOrder: -1);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithRequiresAdvanceOrderButNotHighRisk_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            priceOptions,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: true);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Execute_WithInvalidMinimumAdvanceOrderQuantity_ThrowsValidationException(int quantity)
    {
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            priceOptions,
            IsHighRiskItem: true,
            RequiresAdvanceOrder: true,
            MinimumAdvanceOrderQuantity: quantity);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithMinimumQuantityButNoAdvanceOrder_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            priceOptions,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: 5);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNotAlwaysAvailableAndNoAvailableDays_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            priceOptions,
            IsAlwaysAvailable: false,
            AvailableDays: null);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithInvalidImageUrl_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            priceOptions,
            ImageUrl: "not-a-valid-url");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithAllergenNotesExceedingMaxLength_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptions();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            priceOptions,
            AllergenNotes: new string('a', 501));

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}