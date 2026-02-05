namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemSetDepositOverrideTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly ItemDepositOverrideValidator _depositOverrideValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly ItemDepositOverrideVO.Create _depositOverrideCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.SetDepositOverride _setDepositOverride;

    public MenuItemSetDepositOverrideTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _depositOverrideCreate = new(_depositOverrideValidator);
        _create = new(_priceOptionCreate, _validator);
        _setDepositOverride = new(_depositOverrideCreate, _validator);
    }

    private static CreatePriceOptionCommand[] CreateValidPriceOptionCommands() =>
    [
        new CreatePriceOptionCommand(PortionType.Full, 10.99m)
    ];

    private MenuItemAgg CreateMenuItem(string name = "Test MenuItem")
    {
        var command = new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
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

        return _create.Execute(command);
    }

    #region Success Tests

    [Fact]
    public void Execute_WithValidDepositAmount_SetsDepositOverride()
    {
        var menuItem = CreateMenuItem();
        var command = new SetDepositOverrideCommand(DepositAmount: 30.00m);

        var result = _setDepositOverride.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.HasDepositOverride.Should().BeTrue();
        result.DepositOverride.Should().NotBeNull();
        result.DepositOverride!.DepositAmount.Should().Be(30.00m);
    }

    [Fact]
    public void Execute_WithoutMinimumQuantity_SetsAppliesToAllQuantities()
    {
        var menuItem = CreateMenuItem();
        var command = new SetDepositOverrideCommand(DepositAmount: 30.00m, MinimumQuantityForDeposit: null);

        var result = _setDepositOverride.Execute(menuItem, command);

        result.DepositOverride!.AppliesToAllQuantities.Should().BeTrue();
        result.DepositOverride.MinimumQuantityForDeposit.Should().BeNull();
    }

    [Fact]
    public void Execute_WithMinimumQuantity_SetsMinimumQuantityForDeposit()
    {
        var menuItem = CreateMenuItem();
        var command = new SetDepositOverrideCommand(DepositAmount: 30.00m, MinimumQuantityForDeposit: 4);

        var result = _setDepositOverride.Execute(menuItem, command);

        result.DepositOverride!.MinimumQuantityForDeposit.Should().Be(4);
        result.DepositOverride.AppliesToAllQuantities.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithMinimumQuantity_IsApplicableWorksCorrectly()
    {
        var menuItem = CreateMenuItem();
        var command = new SetDepositOverrideCommand(DepositAmount: 30.00m, MinimumQuantityForDeposit: 4);

        var result = _setDepositOverride.Execute(menuItem, command);

        result.DepositOverride!.IsApplicable(5).Should().BeTrue();
        result.DepositOverride.IsApplicable(4).Should().BeTrue();
        result.DepositOverride.IsApplicable(3).Should().BeFalse();
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        var createCommand = new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
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
        var menuItem = _create.Execute(createCommand);
        var command = new SetDepositOverrideCommand(DepositAmount: 30.00m);

        var result = _setDepositOverride.Execute(menuItem, command);

        result.Name.Should().Be("Test MenuItem");
        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Execute_ReplacesExistingDepositOverride()
    {
        var menuItem = CreateMenuItem();
        var firstCommand = new SetDepositOverrideCommand(DepositAmount: 20.00m, MinimumQuantityForDeposit: 2);
        var secondCommand = new SetDepositOverrideCommand(DepositAmount: 50.00m, MinimumQuantityForDeposit: 5);

        var firstResult = _setDepositOverride.Execute(menuItem, firstCommand);
        var secondResult = _setDepositOverride.Execute(firstResult, secondCommand);

        secondResult.DepositOverride!.DepositAmount.Should().Be(50.00m);
        secondResult.DepositOverride.MinimumQuantityForDeposit.Should().Be(5);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Execute_WithZeroDepositAmount_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new SetDepositOverrideCommand(DepositAmount: 0m);

        var act = () => _setDepositOverride.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Deposit amount must be greater than zero")));
    }

    [Fact]
    public void Execute_WithNegativeDepositAmount_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new SetDepositOverrideCommand(DepositAmount: -10.00m);

        var act = () => _setDepositOverride.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Deposit amount must be greater than zero")));
    }

    [Fact]
    public void Execute_WithDepositAmountExceedingMax_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new SetDepositOverrideCommand(DepositAmount: 10001m);

        var act = () => _setDepositOverride.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Deposit amount cannot exceed 10000")));
    }

    [Fact]
    public void Execute_WithZeroMinimumQuantity_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new SetDepositOverrideCommand(DepositAmount: 30.00m, MinimumQuantityForDeposit: 0);

        var act = () => _setDepositOverride.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Minimum quantity must be at least 1")));
    }

    [Fact]
    public void Execute_WithMinimumQuantityExceedingMax_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new SetDepositOverrideCommand(DepositAmount: 30.00m, MinimumQuantityForDeposit: 101);

        var act = () => _setDepositOverride.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Minimum quantity cannot exceed 100")));
    }

    #endregion
}
