namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemMarkAsUnavailableTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.Activate _activate;
    private readonly MenuItemAgg.MarkAsUnavailable _markAsUnavailable;

    public MenuItemMarkAsUnavailableTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _create = new(_priceOptionCreate, _validator);
        _activate = new(_validator);
        _markAsUnavailable = new(_validator);
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

    private MenuItemAgg CreateActiveMenuItem(string name = "Active MenuItem")
    {
        var menuItem = CreateMenuItem(name);
        return _activate.Execute(menuItem, new ActivateMenuItemCommand());
    }

    #region Success Tests

    [Fact]
    public void Execute_WithAvailableMenuItem_MarksAsUnavailable()
    {
        var menuItem = CreateMenuItem();
        var command = new MarkMenuItemAsUnavailableCommand();

        var result = _markAsUnavailable.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithMenuItem_ReturnsValidatedMenuItem()
    {
        var menuItem = CreateMenuItem(name: "Pulpo al Horno");
        var command = new MarkMenuItemAsUnavailableCommand();

        var result = _markAsUnavailable.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.Name.Should().Be("Pulpo al Horno");
        result.IsAvailable.Should().BeFalse();
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
        var command = new MarkMenuItemAsUnavailableCommand();

        var result = _markAsUnavailable.Execute(menuItem, command);

        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_PreservesPriceOptions()
    {
        var menuItem = CreateMenuItem();
        var command = new MarkMenuItemAsUnavailableCommand();

        var result = _markAsUnavailable.Execute(menuItem, command);

        result.PriceOptions.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_WithActiveMenuItem_UpdatesCanBeOrdered()
    {
        var menuItem = CreateActiveMenuItem();
        menuItem.CanBeOrdered.Should().BeTrue();
        var command = new MarkMenuItemAsUnavailableCommand();

        var result = _markAsUnavailable.Execute(menuItem, command);

        result.CanBeOrdered.Should().BeFalse();
    }

    #endregion

    #region Idempotent Tests

    [Fact]
    public void Execute_WithUnavailableMenuItem_RemainsUnavailable()
    {
        var menuItem = CreateMenuItem();
        var command = new MarkMenuItemAsUnavailableCommand();
        var unavailableMenuItem = _markAsUnavailable.Execute(menuItem, command);

        var result = _markAsUnavailable.Execute(unavailableMenuItem, command);

        result.Should().NotBeNull();
        result.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithUnavailableMenuItem_DoesNotThrow()
    {
        var menuItem = CreateMenuItem();
        var command = new MarkMenuItemAsUnavailableCommand();
        var unavailableMenuItem = _markAsUnavailable.Execute(menuItem, command);

        var act = () => _markAsUnavailable.Execute(unavailableMenuItem, command);

        act.Should().NotThrow();
    }

    [Fact]
    public void Execute_CalledTwice_ProducesSameResult()
    {
        var menuItem = CreateMenuItem();
        var command = new MarkMenuItemAsUnavailableCommand();

        var firstResult = _markAsUnavailable.Execute(menuItem, command);
        var secondResult = _markAsUnavailable.Execute(firstResult, command);

        secondResult.IsAvailable.Should().BeFalse();
        firstResult.IsAvailable.Should().Be(secondResult.IsAvailable);
    }

    #endregion
}
