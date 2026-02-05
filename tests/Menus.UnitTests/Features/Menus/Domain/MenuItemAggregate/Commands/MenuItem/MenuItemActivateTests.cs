namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemActivateTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _createPriceOption;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.Activate _activate;

    public MenuItemActivateTests()
    {
        _createPriceOption = new(_priceOptionValidator);
        _createMenuItem = new(_createPriceOption, _validator);
        _activate = new(_validator);
    }

    private MenuItemAgg CreateActiveMenuItem(string name = "Active MenuItem")
    {
        var menuItem = _createMenuItem.Execute(new CreateMenuItemCommand(
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
            PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: true)]));
        _activate.Execute(menuItem, new ActivateMenuItemCommand());
        return menuItem;
    }

    private MenuItemAgg CreateInactiveMenuItemWithActivePriceOption(string name = "Inactive MenuItem") =>
        _createMenuItem.Execute(new CreateMenuItemCommand(
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
            PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: true)]));

    private MenuItemAgg CreateInactiveMenuItemWithNoActivePriceOptions(string name = "MenuItem Without Active Price") =>
        _createMenuItem.Execute(new CreateMenuItemCommand(
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
            PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: false)]));

    #region Success Tests

    [Fact]
    public void Execute_WithInactiveMenuItemAndActivePriceOption_ActivatesMenuItem()
    {
        var menuItem = CreateInactiveMenuItemWithActivePriceOption();
        var command = new ActivateMenuItemCommand();

        var result = _activate.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithInactiveMenuItem_ReturnsValidatedMenuItem()
    {
        var menuItem = CreateInactiveMenuItemWithActivePriceOption(name: "Pulpo al Horno");
        var command = new ActivateMenuItemCommand();

        var result = _activate.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.Name.Should().Be("Pulpo al Horno");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        var menuItem = _createMenuItem.Execute(new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Item",
            Description: "Test Description",
            ImageUrl: "https://example.com/image.jpg",
            DisplayOrder: 5,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: true)]));
        var command = new ActivateMenuItemCommand();

        var result = _activate.Execute(menuItem, command);

        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Execute_PreservesPriceOptions()
    {
        var menuItem = CreateInactiveMenuItemWithActivePriceOption();
        var command = new ActivateMenuItemCommand();

        var result = _activate.Execute(menuItem, command);

        result.PriceOptions.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_WithMultipleActivePriceOptions_ActivatesMenuItem()
    {
        var menuItem = _createMenuItem.Execute(new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: [
                new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: true),
                new CreatePriceOptionCommand(PortionType.Half, 7.00m, IsActive: true)
            ]));
        var command = new ActivateMenuItemCommand();

        var result = _activate.Execute(menuItem, command);

        result.IsActive.Should().BeTrue();
        result.PriceOptions.Should().HaveCount(2);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public void Execute_WithActiveMenuItem_ThrowsConflictException()
    {
        var menuItem = CreateActiveMenuItem();
        var command = new ActivateMenuItemCommand();

        var act = () => _activate.Execute(menuItem, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("Menu item is already active");
    }

    [Fact]
    public void Execute_WithActiveMenuItem_DoesNotModifyMenuItem()
    {
        var menuItem = CreateActiveMenuItem();
        var originalIsActive = menuItem.IsActive;
        var command = new ActivateMenuItemCommand();

        try
        {
            _activate.Execute(menuItem, command);
        }
        catch (ConflictException)
        {
            // Expected
        }

        menuItem.IsActive.Should().Be(originalIsActive);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Execute_WithNoActivePriceOptions_ThrowsValidationException()
    {
        var menuItem = CreateInactiveMenuItemWithNoActivePriceOptions();
        var command = new ActivateMenuItemCommand();

        var act = () => _activate.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Menu item must have at least one active price option")));
    }

    [Fact]
    public void Execute_WithAllPriceOptionsInactive_ThrowsValidationException()
    {
        var menuItem = _createMenuItem.Execute(new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: [
                new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: false),
                new CreatePriceOptionCommand(PortionType.Half, 7.00m, IsActive: false)
            ]));
        var command = new ActivateMenuItemCommand();

        var act = () => _activate.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithMixedPriceOptions_ActivatesIfAtLeastOneActive()
    {
        var menuItem = _createMenuItem.Execute(new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: [
                new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: true),
                new CreatePriceOptionCommand(PortionType.Half, 7.00m, IsActive: false)
            ]));
        var command = new ActivateMenuItemCommand();

        var result = _activate.Execute(menuItem, command);

        result.IsActive.Should().BeTrue();
    }

    #endregion
}
