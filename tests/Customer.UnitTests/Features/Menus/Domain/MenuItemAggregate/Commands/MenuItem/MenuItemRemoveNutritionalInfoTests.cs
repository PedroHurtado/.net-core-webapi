namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemRemoveNutritionalInfoTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly NutritionalInfoValidator _nutritionalInfoValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly NutritionalInfoVO.Create _nutritionalInfoCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.SetNutritionalInfo _setNutritionalInfo;
    private readonly MenuItemAgg.RemoveNutritionalInfo _removeNutritionalInfo;

    public MenuItemRemoveNutritionalInfoTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _nutritionalInfoCreate = new(_nutritionalInfoValidator);
        _create = new(_priceOptionCreate, _validator);
        _setNutritionalInfo = new(_nutritionalInfoCreate, _validator);
        _removeNutritionalInfo = new(_validator);
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

    private MenuItemAgg CreateMenuItemWithNutritionalInfo(
        string name = "Test MenuItem",
        int calories = 500,
        decimal protein = 25.0m,
        decimal carbohydrates = 40.0m,
        decimal fat = 15.0m,
        int servingSize = 350)
    {
        var menuItem = CreateMenuItem(name);
        return _setNutritionalInfo.Execute(menuItem, new SetNutritionalInfoCommand(
            calories, protein, carbohydrates, fat, servingSize));
    }

    #region Success Tests

    [Fact]
    public void Execute_WithExistingNutritionalInfo_RemovesNutritionalInfo()
    {
        var menuItem = CreateMenuItemWithNutritionalInfo();
        menuItem.NutritionalInfo.Should().NotBeNull();
        var command = new RemoveNutritionalInfoCommand();

        var result = _removeNutritionalInfo.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.NutritionalInfo.Should().BeNull();
    }

    [Fact]
    public void Execute_WithExistingNutritionalInfo_ReturnsValidatedMenuItem()
    {
        var menuItem = CreateMenuItemWithNutritionalInfo(name: "Pulpo al Horno");
        var command = new RemoveNutritionalInfoCommand();

        var result = _removeNutritionalInfo.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.Name.Should().Be("Pulpo al Horno");
        result.NutritionalInfo.Should().BeNull();
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
        menuItem = _setNutritionalInfo.Execute(menuItem, new SetNutritionalInfoCommand(500, 25.0m, 40.0m, 15.0m, 350));
        var command = new RemoveNutritionalInfoCommand();

        var result = _removeNutritionalInfo.Execute(menuItem, command);

        result.Name.Should().Be("Test MenuItem");
        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Execute_PreservesPriceOptions()
    {
        var menuItem = CreateMenuItemWithNutritionalInfo();
        var command = new RemoveNutritionalInfoCommand();

        var result = _removeNutritionalInfo.Execute(menuItem, command);

        result.PriceOptions.Should().HaveCount(1);
    }

    #endregion

    #region Idempotent Tests

    [Fact]
    public void Execute_WithoutNutritionalInfo_RemainsWithoutNutritionalInfo()
    {
        var menuItem = CreateMenuItem();
        menuItem.NutritionalInfo.Should().BeNull();
        var command = new RemoveNutritionalInfoCommand();

        var result = _removeNutritionalInfo.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.NutritionalInfo.Should().BeNull();
    }

    [Fact]
    public void Execute_WithoutNutritionalInfo_DoesNotThrow()
    {
        var menuItem = CreateMenuItem();
        var command = new RemoveNutritionalInfoCommand();

        var act = () => _removeNutritionalInfo.Execute(menuItem, command);

        act.Should().NotThrow();
    }

    [Fact]
    public void Execute_CalledTwice_ProducesSameResult()
    {
        var menuItem = CreateMenuItemWithNutritionalInfo();
        var command = new RemoveNutritionalInfoCommand();

        var firstResult = _removeNutritionalInfo.Execute(menuItem, command);
        var secondResult = _removeNutritionalInfo.Execute(firstResult, command);

        secondResult.NutritionalInfo.Should().BeNull();
        firstResult.NutritionalInfo.Should().Be(secondResult.NutritionalInfo);
    }

    #endregion
}
