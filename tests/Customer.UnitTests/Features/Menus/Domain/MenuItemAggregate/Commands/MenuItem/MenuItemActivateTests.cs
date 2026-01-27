namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemActivateTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Activate _activate;

    public MenuItemActivateTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _activate = new(_validator);
    }

    private TestableMenuItem CreateInactiveMenuItemWithActivePriceOption(string name = "Inactive MenuItem")
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = name,
            IsActive = false,
            IsAvailable = true,
            IsAlwaysAvailable = true
        };

        var priceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: true));
        menuItem.AddPriceOption(priceOption);

        return menuItem;
    }

    private TestableMenuItem CreateActiveMenuItem(string name = "Active MenuItem")
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = name,
            IsActive = true,
            IsAvailable = true,
            IsAlwaysAvailable = true
        };

        var priceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: true));
        menuItem.AddPriceOption(priceOption);

        return menuItem;
    }

    private TestableMenuItem CreateInactiveMenuItemWithNoActivePriceOptions(string name = "MenuItem Without Active Price")
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = name,
            IsActive = false,
            IsAvailable = true,
            IsAlwaysAvailable = true
        };

        var priceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: false));
        menuItem.AddPriceOption(priceOption);

        return menuItem;
    }

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
        var menuItem = CreateInactiveMenuItemWithActivePriceOption();
        menuItem.Description = "Test Description";
        menuItem.ImageUrl = "https://example.com/image.jpg";
        menuItem.DisplayOrder = 5;
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
        var menuItem = CreateInactiveMenuItemWithActivePriceOption();
        var secondPriceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Half, 7.00m, IsActive: true));
        menuItem.AddPriceOption(secondPriceOption);
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
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Test Item",
            IsActive = false,
            IsAvailable = true,
            IsAlwaysAvailable = true
        };
        var inactivePriceOption1 = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: false));
        var inactivePriceOption2 = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Half, 7.00m, IsActive: false));
        menuItem.AddPriceOption(inactivePriceOption1);
        menuItem.AddPriceOption(inactivePriceOption2);
        var command = new ActivateMenuItemCommand();

        var act = () => _activate.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithMixedPriceOptions_ActivatesIfAtLeastOneActive()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Test Item",
            IsActive = false,
            IsAvailable = true,
            IsAlwaysAvailable = true
        };
        var activePriceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 10.99m, IsActive: true));
        var inactivePriceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Half, 7.00m, IsActive: false));
        menuItem.AddPriceOption(activePriceOption);
        menuItem.AddPriceOption(inactivePriceOption);
        var command = new ActivateMenuItemCommand();

        var result = _activate.Execute(menuItem, command);

        result.IsActive.Should().BeTrue();
    }

    #endregion
}
