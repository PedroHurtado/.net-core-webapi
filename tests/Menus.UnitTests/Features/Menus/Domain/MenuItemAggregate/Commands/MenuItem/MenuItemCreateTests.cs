namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemCreateTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOption.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _create;

    public MenuItemCreateTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _create = new(_priceOptionCreate, _validator);
    }

    private static CreatePriceOptionCommand[] CreateValidPriceOptionCommands() =>
    [
        new CreatePriceOptionCommand(PortionType.Full, 10.99m)
    ];

    [Fact]
    public void Execute_WithValidCommand_ReturnsMenuItem()
    {
        var tenantId = Guid.NewGuid();
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            tenantId,
            "Caesar Salad",
            null,
            null,
            0,
            false,
            false,
            null,
            true,
            [],
            null,
            priceOptions);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.TenantId.Should().Be(tenantId);
        result.Name.Should().Be("Caesar Salad");
        result.Description.Should().BeNull();
        result.ImageUrl.Should().BeNull();
        result.DisplayOrder.Should().Be(0);
        result.IsActive.Should().BeFalse();
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
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            tenantId,
            "Wagyu Steak",
            "Premium Japanese beef",
            "https://example.com/wagyu.jpg",
            5,
            true,
            true,
            10,
            true,
            [],
            "May contain traces of soy",
            priceOptions);

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
        CreatePriceOptionCommand[] priceOptions =
        [
            new CreatePriceOptionCommand(PortionType.Full, 15.99m),
            new CreatePriceOptionCommand(PortionType.Half, 9.99m)
        ];
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Pasta",
            null,
            null,
            0,
            false,
            false,
            null,
            true,
            [],
            null,
            priceOptions);

        var result = _create.Execute(command);

        result.PriceOptions.Should().HaveCount(2);
    }

    [Fact]
    public void Execute_WithAvailableDays_SetsAvailableDays()
    {
        var priceOptions = CreateValidPriceOptionCommands();
        DayOfWeek[] availableDays = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Weekly Special",
            null,
            null,
            0,
            false,
            false,
            null,
            false,
            availableDays,
            null,
            priceOptions);

        var result = _create.Execute(command);

        result.IsAlwaysAvailable.Should().BeFalse();
        result.AvailableDays.Should().BeEquivalentTo(availableDays);
    }

    #region Validation Throws

    [Fact]
    public void Execute_WithEmptyTenantId_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            Guid.Empty,
            "Item",
            null,
            null,
            0,
            false,
            false,
            null,
            true,
            [],
            null,
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            name!,
            null,
            null,
            0,
            false,
            false,
            null,
            true,
            [],
            null,
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            new string('a', 101),
            null,
            null,
            0,
            false,
            false,
            null,
            true,
            [],
            null,
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            new string('a', 1001),
            null,
            0,
            false,
            false,
            null,
            true,
            [],
            null,
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithEmptyPriceOptions_ThrowsValidationException()
    {
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            null,
            null,
            0,
            false,
            false,
            null,
            true,
            [],
            null,
            []);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            null,
            null,
            -1,
            false,
            false,
            null,
            true,
            [],
            null,
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithRequiresAdvanceOrderButNotHighRisk_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            null,
            null,
            0,
            false,
            true,
            null,
            true,
            [],
            null,
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Execute_WithInvalidMinimumAdvanceOrderQuantity_ThrowsValidationException(int quantity)
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            null,
            null,
            0,
            true,
            true,
            quantity,
            true,
            [],
            null,
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithMinimumQuantityButNoAdvanceOrder_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            null,
            null,
            0,
            false,
            false,
            5,
            true,
            [],
            null,
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNotAlwaysAvailableAndNoAvailableDays_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            null,
            null,
            0,
            false,
            false,
            null,
            false,
            [],
            null,
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithInvalidImageUrl_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            null,
            "not-a-valid-url",
            0,
            false,
            false,
            null,
            true,
            [],
            null,
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithAllergenNotesExceedingMaxLength_ThrowsValidationException()
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            null,
            null,
            0,
            false,
            false,
            null,
            true,
            [],
            new string('a', 501),
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDuplicatePortionTypes_ThrowsValidationException()
    {
        CreatePriceOptionCommand[] priceOptions =
        [
            new CreatePriceOptionCommand(PortionType.Full, 15.99m),
            new CreatePriceOptionCommand(PortionType.Full, 12.99m)
        ];
        var command = new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Item",
            null,
            null,
            0,
            false,
            false,
            null,
            true,
            [],
            null,
            priceOptions);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
