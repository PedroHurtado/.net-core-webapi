namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemAddPriceOptionTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.AddPriceOption _addPriceOption;

    public MenuItemAddPriceOptionTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _create = new(_priceOptionCreate, _validator);
        _addPriceOption = new(_priceOptionCreate, _validator);
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
    public void Execute_WithValidPriceOption_AddsPriceOption()
    {
        var menuItem = CreateMenuItem();
        var initialCount = menuItem.PriceOptions.Count;
        var command = new AddPriceOptionCommand(PortionType.Half, 7.99m);

        var result = _addPriceOption.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.PriceOptions.Should().HaveCount(initialCount + 1);
        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.Price == 7.99m);
    }

    [Fact]
    public void Execute_WithSmallPortionType_AddsPriceOption()
    {
        var menuItem = CreateMenuItem();
        var command = new AddPriceOptionCommand(PortionType.Small, 5.99m);

        var result = _addPriceOption.Execute(menuItem, command);

        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Small && p.Price == 5.99m);
    }

    [Fact]
    public void Execute_WithMarketPriceAndNoPrice_AddsPriceOption()
    {
        var menuItem = CreateMenuItem();
        var command = new AddPriceOptionCommand(PortionType.MarketPrice, null);

        var result = _addPriceOption.Execute(menuItem, command);

        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.MarketPrice && p.Price == null);
    }

    [Fact]
    public void Execute_WithMarketPriceAndPrice_AddsPriceOption()
    {
        var menuItem = CreateMenuItem();
        var command = new AddPriceOptionCommand(PortionType.MarketPrice, 25.00m);

        var result = _addPriceOption.Execute(menuItem, command);

        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.MarketPrice && p.Price == 25.00m);
    }

    [Fact]
    public void Execute_WithIsActiveFalse_AddsPriceOptionAsInactive()
    {
        var menuItem = CreateMenuItem();
        var command = new AddPriceOptionCommand(PortionType.Half, 7.99m, IsActive: false);

        var result = _addPriceOption.Execute(menuItem, command);

        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && !p.IsActive);
    }

    [Fact]
    public void Execute_WithDefaultIsActive_AddsPriceOptionAsActive()
    {
        var menuItem = CreateMenuItem();
        var command = new AddPriceOptionCommand(PortionType.Half, 7.99m);

        var result = _addPriceOption.Execute(menuItem, command);

        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.IsActive);
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
        var command = new AddPriceOptionCommand(PortionType.Half, 7.99m);

        var result = _addPriceOption.Execute(menuItem, command);

        result.Name.Should().Be("Test MenuItem");
        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Execute_PreservesExistingPriceOptions()
    {
        var menuItem = CreateMenuItem();
        var existingOption = menuItem.PriceOptions.First();
        var command = new AddPriceOptionCommand(PortionType.Half, 7.99m);

        var result = _addPriceOption.Execute(menuItem, command);

        result.PriceOptions.Should().Contain(p =>
            p.PortionType == existingOption.PortionType &&
            p.Price == existingOption.Price);
    }

    [Fact]
    public void Execute_WithMultipleAdds_AddsAllPriceOptions()
    {
        var menuItem = CreateMenuItem();
        var halfCommand = new AddPriceOptionCommand(PortionType.Half, 7.99m);
        var smallCommand = new AddPriceOptionCommand(PortionType.Small, 5.99m);

        var result1 = _addPriceOption.Execute(menuItem, halfCommand);
        var result2 = _addPriceOption.Execute(result1, smallCommand);

        result2.PriceOptions.Should().HaveCount(3);
        result2.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full);
        result2.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half);
        result2.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Small);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public void Execute_WithDuplicatePortionType_ThrowsConflictException()
    {
        var menuItem = CreateMenuItem();
        var command = new AddPriceOptionCommand(PortionType.Full, 15.99m);

        var act = () => _addPriceOption.Execute(menuItem, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Price option with portion type 'Full' already exists*");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Execute_WithNegativePrice_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new AddPriceOptionCommand(PortionType.Half, -5.00m);

        var act = () => _addPriceOption.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Price cannot be negative")));
    }

    [Fact]
    public void Execute_WithNullPriceForFixedPortionType_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new AddPriceOptionCommand(PortionType.Half, null);

        var act = () => _addPriceOption.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Price is required for fixed portion types")));
    }

    [Fact]
    public void Execute_WithZeroPrice_Succeeds()
    {
        var menuItem = CreateMenuItem();
        var command = new AddPriceOptionCommand(PortionType.Half, 0m);

        var result = _addPriceOption.Execute(menuItem, command);

        result.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.Price == 0m);
    }

    #endregion
}
