namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemRemoveAllergenTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly AllergenValidator _allergenValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly Allergen.Create _allergenCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.AddAllergen _addAllergen;
    private readonly MenuItemAgg.RemoveAllergen _removeAllergen;

    public MenuItemRemoveAllergenTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _allergenCreate = new(_allergenValidator);
        _create = new(_priceOptionCreate, _validator);
        _addAllergen = new(_validator);
        _removeAllergen = new(_validator);
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

    private Allergen CreateAllergen(string code = "GLUTEN", string name = "Gluten")
    {
        var command = new CreateAllergenCommand(
            Code: code,
            Name: name,
            IconUrl: null,
            IsActive: true,
            DisplayOrder: 0
        );
        return _allergenCreate.Execute(command);
    }

    private MenuItemAgg CreateMenuItemWithAllergen(string allergenCode = "GLUTEN", string allergenName = "Gluten")
    {
        var menuItem = CreateMenuItem();
        var allergen = CreateAllergen(allergenCode, allergenName);
        return _addAllergen.Execute(menuItem, new AddAllergenCommand(allergen));
    }

    #region Success Tests

    [Fact]
    public void Execute_WithExistingAllergen_RemovesAllergen()
    {
        // Arrange
        var menuItem = CreateMenuItemWithAllergen("GLUTEN", "Gluten");

        // Act
        var result = _removeAllergen.Execute(menuItem, new RemoveAllergenCommand("GLUTEN"));

        // Assert
        result.Should().NotBeNull();
        result.Allergens.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithMultipleAllergens_RemovesOnlySpecified()
    {
        // Arrange
        var menuItem = CreateMenuItemWithAllergen("GLUTEN", "Gluten");
        var lactoseAllergen = CreateAllergen("LACTOSE", "Lactosa");
        menuItem = _addAllergen.Execute(menuItem, new AddAllergenCommand(lactoseAllergen));

        // Act
        var result = _removeAllergen.Execute(menuItem, new RemoveAllergenCommand("GLUTEN"));

        // Assert
        result.Allergens.Should().ContainSingle();
        result.Allergens.Should().Contain(a => a.Id == "LACTOSE");
        result.Allergens.Should().NotContain(a => a.Id == "GLUTEN");
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        // Arrange
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
        var allergen = CreateAllergen();
        menuItem = _addAllergen.Execute(menuItem, new AddAllergenCommand(allergen));

        // Act
        var result = _removeAllergen.Execute(menuItem, new RemoveAllergenCommand("GLUTEN"));

        // Assert
        result.Name.Should().Be("Test MenuItem");
        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
    }

    #endregion

    #region NotFound Tests

    [Fact]
    public void Execute_WithNonExistingAllergen_ThrowsKeyNotFoundException()
    {
        // Arrange
        var menuItem = CreateMenuItem();

        // Act
        var act = () => _removeAllergen.Execute(menuItem, new RemoveAllergenCommand("SULFITOS"));

        // Assert
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("Allergen not found in this item");
    }

    [Fact]
    public void Execute_WithWrongAllergenId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var menuItem = CreateMenuItemWithAllergen("GLUTEN", "Gluten");

        // Act
        var act = () => _removeAllergen.Execute(menuItem, new RemoveAllergenCommand("LACTOSE"));

        // Assert
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("Allergen not found in this item");
    }

    #endregion
}
