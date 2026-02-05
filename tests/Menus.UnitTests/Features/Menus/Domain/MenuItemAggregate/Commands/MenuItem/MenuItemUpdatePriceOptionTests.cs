namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemUpdatePriceOptionTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.AddPriceOption _addPriceOption;
    private readonly MenuItemAgg.UpdatePriceOption _updatePriceOption;

    public MenuItemUpdatePriceOptionTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _create = new(_priceOptionCreate, _validator);
        _addPriceOption = new(_priceOptionCreate, _validator);
        _updatePriceOption = new(_priceOptionCreate, _validator);
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

    private MenuItemAgg CreateMenuItemWithMultiplePriceOptions()
    {
        var menuItem = CreateMenuItem();
        _addPriceOption.Execute(menuItem, new AddPriceOptionCommand(PortionType.Half, 7.99m));
        return menuItem;
    }

    #region Success Tests

    [Fact]
    public void Execute_WithValidUpdate_UpdatesPriceOption()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdatePriceOptionCommand(PortionType.Full, 15.99m, true);

        var result = _updatePriceOption.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.PriceOptions.Should().HaveCount(1);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full && p.Price == 15.99m);
    }

    [Fact]
    public void Execute_WithNewPrice_UpdatesPrice()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdatePriceOptionCommand(PortionType.Full, 25.00m, true);

        var result = _updatePriceOption.Execute(menuItem, command);

        result.PriceOptions.First(p => p.PortionType == PortionType.Full).Price.Should().Be(25.00m);
    }

    [Fact]
    public void Execute_DeactivatingWithOtherActiveOptions_Succeeds()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        var command = new UpdatePriceOptionCommand(PortionType.Full, 10.99m, false);

        var result = _updatePriceOption.Execute(menuItem, command);

        result.PriceOptions.First(p => p.PortionType == PortionType.Full).IsActive.Should().BeFalse();
        result.PriceOptions.First(p => p.PortionType == PortionType.Half).IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_ActivatingInactiveOption_Succeeds()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        _updatePriceOption.Execute(menuItem, new UpdatePriceOptionCommand(PortionType.Full, 10.99m, false));
        var command = new UpdatePriceOptionCommand(PortionType.Full, 10.99m, true);

        var result = _updatePriceOption.Execute(menuItem, command);

        result.PriceOptions.First(p => p.PortionType == PortionType.Full).IsActive.Should().BeTrue();
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
        var command = new UpdatePriceOptionCommand(PortionType.Full, 15.99m, true);

        var result = _updatePriceOption.Execute(menuItem, command);

        result.Name.Should().Be("Test MenuItem");
        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Execute_PreservesOtherPriceOptions()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        var command = new UpdatePriceOptionCommand(PortionType.Full, 15.99m, true);

        var result = _updatePriceOption.Execute(menuItem, command);

        result.PriceOptions.Should().HaveCount(2);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.Price == 7.99m);
    }

    [Fact]
    public void Execute_WithZeroPrice_Succeeds()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdatePriceOptionCommand(PortionType.Full, 0m, true);

        var result = _updatePriceOption.Execute(menuItem, command);

        result.PriceOptions.First(p => p.PortionType == PortionType.Full).Price.Should().Be(0m);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public void Execute_WithNonExistentPortionType_ThrowsKeyNotFoundException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdatePriceOptionCommand(PortionType.Half, 7.99m, true);

        var act = () => _updatePriceOption.Execute(menuItem, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Price option with portion type 'Half' not found*");
    }

    [Fact]
    public void Execute_WithNonExistentSmallPortionType_ThrowsKeyNotFoundException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdatePriceOptionCommand(PortionType.Small, 5.99m, true);

        var act = () => _updatePriceOption.Execute(menuItem, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Price option with portion type 'Small' not found*");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Execute_DeactivatingLastActiveOption_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdatePriceOptionCommand(PortionType.Full, 10.99m, false);

        var act = () => _updatePriceOption.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Cannot deactivate last active price option")));
    }

    [Fact]
    public void Execute_DeactivatingLastActiveWhenOthersInactive_ThrowsValidationException()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        _updatePriceOption.Execute(menuItem, new UpdatePriceOptionCommand(PortionType.Half, 7.99m, false));
        var command = new UpdatePriceOptionCommand(PortionType.Full, 10.99m, false);

        var act = () => _updatePriceOption.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Cannot deactivate last active price option")));
    }

    [Fact]
    public void Execute_WithNegativePrice_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdatePriceOptionCommand(PortionType.Full, -5.00m, true);

        var act = () => _updatePriceOption.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Price cannot be negative")));
    }

    [Fact]
    public void Execute_WithNullPriceForFixedPortionType_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdatePriceOptionCommand(PortionType.Full, null, true);

        var act = () => _updatePriceOption.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Price is required for fixed portion types")));
    }

    #endregion
}
