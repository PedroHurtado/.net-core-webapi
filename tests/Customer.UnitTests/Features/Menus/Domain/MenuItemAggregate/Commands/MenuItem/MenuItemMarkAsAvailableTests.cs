namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemMarkAsAvailableTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.MarkAsAvailable _markAsAvailable;

    public MenuItemMarkAsAvailableTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _create = new(_priceOptionCreate, _validator);
        _markAsAvailable = new(_validator);
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
    public void Execute_WithAvailableMenuItem_RemainsAvailable()
    {
        var menuItem = CreateMenuItem();
        var command = new MarkMenuItemAsAvailableCommand();

        var result = _markAsAvailable.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithMenuItem_ReturnsValidatedMenuItem()
    {
        var menuItem = CreateMenuItem(name: "Pulpo al Horno");
        var command = new MarkMenuItemAsAvailableCommand();

        var result = _markAsAvailable.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.Name.Should().Be("Pulpo al Horno");
        result.IsAvailable.Should().BeTrue();
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
        var command = new MarkMenuItemAsAvailableCommand();

        var result = _markAsAvailable.Execute(menuItem, command);

        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_PreservesPriceOptions()
    {
        var menuItem = CreateMenuItem();
        var command = new MarkMenuItemAsAvailableCommand();

        var result = _markAsAvailable.Execute(menuItem, command);

        result.PriceOptions.Should().HaveCount(1);
    }

    #endregion

    #region Idempotent Tests

    [Fact]
    public void Execute_WithAvailableMenuItem_DoesNotThrow()
    {
        var menuItem = CreateMenuItem();
        var command = new MarkMenuItemAsAvailableCommand();

        var act = () => _markAsAvailable.Execute(menuItem, command);

        act.Should().NotThrow();
    }

    [Fact]
    public void Execute_CalledTwice_ProducesSameResult()
    {
        var menuItem = CreateMenuItem();
        var command = new MarkMenuItemAsAvailableCommand();

        var firstResult = _markAsAvailable.Execute(menuItem, command);
        var secondResult = _markAsAvailable.Execute(firstResult, command);

        secondResult.IsAvailable.Should().BeTrue();
        firstResult.IsAvailable.Should().Be(secondResult.IsAvailable);
    }

    #endregion
}
