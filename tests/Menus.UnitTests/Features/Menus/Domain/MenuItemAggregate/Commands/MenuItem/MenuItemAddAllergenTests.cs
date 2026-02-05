namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemAddAllergenTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly AllergenValidator _allergenValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly Allergen.Create _allergenCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.AddAllergen _addAllergen;

    public MenuItemAddAllergenTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _allergenCreate = new(_allergenValidator);
        _create = new(_priceOptionCreate, _validator);
        _addAllergen = new(_validator);
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

    #region Success Tests

    [Fact]
    public void Execute_WithValidAllergen_AddsAllergen()
    {
        var menuItem = CreateMenuItem();
        var allergen = CreateAllergen("GLUTEN", "Gluten");
        var command = new AddAllergenCommand(allergen);

        var result = _addAllergen.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.Allergens.Should().ContainSingle();
        result.Allergens.Should().Contain(a => a.Id == "GLUTEN");
    }

    [Fact]
    public void Execute_WithMultipleAllergens_AddsAllAllergens()
    {
        var menuItem = CreateMenuItem();
        var glutenAllergen = CreateAllergen("GLUTEN", "Gluten");
        var lactoseAllergen = CreateAllergen("LACTOSE", "Lactosa");

        var result1 = _addAllergen.Execute(menuItem, new AddAllergenCommand(glutenAllergen));
        var result2 = _addAllergen.Execute(result1, new AddAllergenCommand(lactoseAllergen));

        result2.Allergens.Should().HaveCount(2);
        result2.Allergens.Should().Contain(a => a.Id == "GLUTEN");
        result2.Allergens.Should().Contain(a => a.Id == "LACTOSE");
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
        var allergen = CreateAllergen();
        var command = new AddAllergenCommand(allergen);

        var result = _addAllergen.Execute(menuItem, command);

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
        var allergen = CreateAllergen();
        var command = new AddAllergenCommand(allergen);

        var result = _addAllergen.Execute(menuItem, command);

        result.PriceOptions.Should().Contain(p =>
            p.PortionType == existingOption.PortionType &&
            p.Price == existingOption.Price);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public void Execute_WithDuplicateAllergen_ThrowsConflictException()
    {
        var menuItem = CreateMenuItem();
        var allergen = CreateAllergen("GLUTEN", "Gluten");
        var result = _addAllergen.Execute(menuItem, new AddAllergenCommand(allergen));

        var duplicateAllergen = CreateAllergen("GLUTEN", "Gluten");
        var act = () => _addAllergen.Execute(result, new AddAllergenCommand(duplicateAllergen));

        act.Should().Throw<ConflictException>()
            .WithMessage("Allergen already exists in this item");
    }

    #endregion
}