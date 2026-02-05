namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemDeactivateTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _createPriceOption;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly MenuItemAgg.Activate _activate;
    private readonly MenuItemAgg.Deactivate _deactivate;

    public MenuItemDeactivateTests()
    {
        _createPriceOption = new(_priceOptionValidator);
        _createMenuItem = new(_createPriceOption, _validator);
        _activate = new(_validator);
        _deactivate = new(_validator);
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

    private MenuItemAgg CreateInactiveMenuItem(string name = "Inactive MenuItem") =>
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

    #region Success Tests

    [Fact]
    public void Execute_WithActiveMenuItem_DeactivatesMenuItem()
    {
        var menuItem = CreateActiveMenuItem();
        var command = new DeactivateMenuItemCommand();

        var result = _deactivate.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithActiveMenuItem_ReturnsValidatedMenuItem()
    {
        var menuItem = CreateActiveMenuItem(name: "Pulpo al Horno");
        var command = new DeactivateMenuItemCommand();

        var result = _deactivate.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.Name.Should().Be("Pulpo al Horno");
        result.IsActive.Should().BeFalse();
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
        _activate.Execute(menuItem, new ActivateMenuItemCommand());
        var command = new DeactivateMenuItemCommand();

        var result = _deactivate.Execute(menuItem, command);

        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Execute_PreservesPriceOptions()
    {
        var menuItem = CreateActiveMenuItem();
        var command = new DeactivateMenuItemCommand();

        var result = _deactivate.Execute(menuItem, command);

        result.PriceOptions.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_UpdatesCanBeOrdered()
    {
        var menuItem = CreateActiveMenuItem();
        menuItem.CanBeOrdered.Should().BeTrue();
        var command = new DeactivateMenuItemCommand();

        var result = _deactivate.Execute(menuItem, command);

        result.CanBeOrdered.Should().BeFalse();
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public void Execute_WithInactiveMenuItem_ThrowsConflictException()
    {
        var menuItem = CreateInactiveMenuItem();
        var command = new DeactivateMenuItemCommand();

        var act = () => _deactivate.Execute(menuItem, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("Menu item is already inactive");
    }

    [Fact]
    public void Execute_WithInactiveMenuItem_DoesNotModifyMenuItem()
    {
        var menuItem = CreateInactiveMenuItem();
        var originalIsActive = menuItem.IsActive;
        var command = new DeactivateMenuItemCommand();

        try
        {
            _deactivate.Execute(menuItem, command);
        }
        catch (ConflictException)
        {
            // Expected
        }

        menuItem.IsActive.Should().Be(originalIsActive);
    }

    #endregion
}
