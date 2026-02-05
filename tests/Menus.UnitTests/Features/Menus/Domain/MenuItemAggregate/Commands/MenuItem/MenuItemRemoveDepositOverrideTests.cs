namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemRemoveDepositOverrideTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly ItemDepositOverrideValidator _depositOverrideValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly ItemDepositOverrideVO.Create _depositOverrideCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.SetDepositOverride _setDepositOverride;
    private readonly MenuItemAgg.RemoveDepositOverride _removeDepositOverride;

    public MenuItemRemoveDepositOverrideTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _depositOverrideCreate = new(_depositOverrideValidator);
        _create = new(_priceOptionCreate, _validator);
        _setDepositOverride = new(_depositOverrideCreate, _validator);
        _removeDepositOverride = new(_validator);
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

    private MenuItemAgg CreateMenuItemWithDepositOverride(
        string name = "Test MenuItem",
        decimal depositAmount = 30.00m,
        int? minimumQuantity = null)
    {
        var menuItem = CreateMenuItem(name);
        return _setDepositOverride.Execute(menuItem, new SetDepositOverrideCommand(depositAmount, minimumQuantity));
    }

    #region Success Tests

    [Fact]
    public void Execute_WithExistingDepositOverride_RemovesDepositOverride()
    {
        var menuItem = CreateMenuItemWithDepositOverride();
        menuItem.HasDepositOverride.Should().BeTrue();
        var command = new RemoveDepositOverrideCommand();

        var result = _removeDepositOverride.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.DepositOverride.Should().BeNull();
        result.HasDepositOverride.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithExistingDepositOverride_ReturnsValidatedMenuItem()
    {
        var menuItem = CreateMenuItemWithDepositOverride(name: "Pulpo al Horno");
        var command = new RemoveDepositOverrideCommand();

        var result = _removeDepositOverride.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.Name.Should().Be("Pulpo al Horno");
        result.HasDepositOverride.Should().BeFalse();
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
        menuItem = _setDepositOverride.Execute(menuItem, new SetDepositOverrideCommand(30.00m));
        var command = new RemoveDepositOverrideCommand();

        var result = _removeDepositOverride.Execute(menuItem, command);

        result.Name.Should().Be("Test MenuItem");
        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Execute_PreservesPriceOptions()
    {
        var menuItem = CreateMenuItemWithDepositOverride();
        var command = new RemoveDepositOverrideCommand();

        var result = _removeDepositOverride.Execute(menuItem, command);

        result.PriceOptions.Should().HaveCount(1);
    }

    #endregion

    #region Idempotent Tests

    [Fact]
    public void Execute_WithoutDepositOverride_RemainsWithoutOverride()
    {
        var menuItem = CreateMenuItem();
        menuItem.HasDepositOverride.Should().BeFalse();
        var command = new RemoveDepositOverrideCommand();

        var result = _removeDepositOverride.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.DepositOverride.Should().BeNull();
        result.HasDepositOverride.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithoutDepositOverride_DoesNotThrow()
    {
        var menuItem = CreateMenuItem();
        var command = new RemoveDepositOverrideCommand();

        var act = () => _removeDepositOverride.Execute(menuItem, command);

        act.Should().NotThrow();
    }

    [Fact]
    public void Execute_CalledTwice_ProducesSameResult()
    {
        var menuItem = CreateMenuItemWithDepositOverride();
        var command = new RemoveDepositOverrideCommand();

        var firstResult = _removeDepositOverride.Execute(menuItem, command);
        var secondResult = _removeDepositOverride.Execute(firstResult, command);

        secondResult.HasDepositOverride.Should().BeFalse();
        firstResult.HasDepositOverride.Should().Be(secondResult.HasDepositOverride);
    }

    #endregion
}
