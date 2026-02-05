namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemRemovePriceOptionTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.AddPriceOption _addPriceOption;
    private readonly MenuItemAgg.RemovePriceOption _removePriceOption;

    public MenuItemRemovePriceOptionTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _create = new(_priceOptionCreate, _validator);
        _addPriceOption = new(_priceOptionCreate, _validator);
        _removePriceOption = new(_validator);
    }

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
            PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, 10.99m)]
        );

        return _create.Execute(command);
    }

    private MenuItemAgg CreateMenuItemWithMultiplePriceOptions()
    {
        var menuItem = CreateMenuItem();
        menuItem = _addPriceOption.Execute(menuItem, new AddPriceOptionCommand(PortionType.Half, 7.99m));
        menuItem = _addPriceOption.Execute(menuItem, new AddPriceOptionCommand(PortionType.Small, 5.99m));
        return menuItem;
    }

    #region Success Tests

    [Fact]
    public void Execute_WithValidPriceOption_RemovesPriceOption()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        var initialCount = menuItem.PriceOptions.Count;
        var command = new RemovePriceOptionCommand(PortionType.Half);

        var result = _removePriceOption.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.PriceOptions.Should().HaveCount(initialCount - 1);
        result.PriceOptions.Should().NotContain(p => p.PortionType == PortionType.Half);
    }

    [Fact]
    public void Execute_WithMultiplePriceOptions_PreservesOtherPriceOptions()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        var command = new RemovePriceOptionCommand(PortionType.Half);

        var result = _removePriceOption.Execute(menuItem, command);

        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full && p.Price == 10.99m);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Small && p.Price == 5.99m);
    }

    [Fact]
    public void Execute_RemovingDifferentPortionTypes_Works()
    {
        var menuItem = CreateMenuItemWithMultiplePriceOptions();
        var command = new RemovePriceOptionCommand(PortionType.Small);

        var result = _removePriceOption.Execute(menuItem, command);

        result.PriceOptions.Should().HaveCount(2);
        result.PriceOptions.Should().NotContain(p => p.PortionType == PortionType.Small);
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
            PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, 10.99m)]
        );
        var menuItem = _create.Execute(createCommand);
        menuItem = _addPriceOption.Execute(menuItem, new AddPriceOptionCommand(PortionType.Half, 7.99m));
        var command = new RemovePriceOptionCommand(PortionType.Half);

        var result = _removePriceOption.Execute(menuItem, command);

        result.Name.Should().Be("Test MenuItem");
        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Execute_WithTwoPriceOptions_CanRemoveOneAndLeaveOne()
    {
        var menuItem = CreateMenuItem();
        menuItem = _addPriceOption.Execute(menuItem, new AddPriceOptionCommand(PortionType.Half, 7.99m));
        var command = new RemovePriceOptionCommand(PortionType.Half);

        var result = _removePriceOption.Execute(menuItem, command);

        result.PriceOptions.Should().HaveCount(1);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public void Execute_WithNonExistentPortionType_ThrowsKeyNotFoundException()
    {
        var menuItem = CreateMenuItem();
        var command = new RemovePriceOptionCommand(PortionType.Half);

        var act = () => _removePriceOption.Execute(menuItem, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Price option with portion type 'Half' not found*");
    }

    [Fact]
    public void Execute_WithNonExistentSmallPortionType_ThrowsKeyNotFoundException()
    {
        var menuItem = CreateMenuItem();
        var command = new RemovePriceOptionCommand(PortionType.Small);

        var act = () => _removePriceOption.Execute(menuItem, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Price option with portion type 'Small' not found*");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Execute_WithLastPriceOption_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new RemovePriceOptionCommand(PortionType.Full);

        var act = () => _removePriceOption.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Cannot remove last price option")));
    }

    #endregion
}
