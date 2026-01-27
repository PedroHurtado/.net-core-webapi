namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemDeactivateTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Deactivate _deactivate;

    public MenuItemDeactivateTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _deactivate = new(_validator);
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

        var priceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 10.99m));
        menuItem.AddPriceOption(priceOption);

        return menuItem;
    }

    private TestableMenuItem CreateInactiveMenuItem(string name = "Inactive MenuItem")
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = name,
            IsActive = false,
            IsAvailable = true,
            IsAlwaysAvailable = true
        };

        var priceOption = _priceOptionCreate.Execute(new CreatePriceOptionCommand(PortionType.Full, 10.99m));
        menuItem.AddPriceOption(priceOption);

        return menuItem;
    }

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
        var menuItem = CreateActiveMenuItem();
        menuItem.Description = "Test Description";
        menuItem.ImageUrl = "https://example.com/image.jpg";
        menuItem.DisplayOrder = 5;
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
