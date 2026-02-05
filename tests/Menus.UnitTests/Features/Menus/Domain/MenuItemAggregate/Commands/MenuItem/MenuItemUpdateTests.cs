namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemUpdateTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly PriceOption.Create _priceOptionCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.Update _update;

    public MenuItemUpdateTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _create = new(_priceOptionCreate, _validator);
        _update = new(_validator);
    }

    private static CreatePriceOptionCommand[] CreateValidPriceOptionCommands() =>
    [
        new CreatePriceOptionCommand(PortionType.Full, 10.99m)
    ];

    private MenuItemAgg CreateMenuItem(
        string name = "Original Name",
        string? description = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isHighRiskItem = false,
        bool requiresAdvanceOrder = false,
        int? minimumAdvanceOrderQuantity = null,
        bool isAlwaysAvailable = true,
        DayOfWeek[]? availableDays = null,
        string? allergenNotes = null)
    {
        var priceOptions = CreateValidPriceOptionCommands();
        var command = new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
            Name: name,
            Description: description,
            ImageUrl: imageUrl,
            DisplayOrder: displayOrder,
            IsHighRiskItem: isHighRiskItem,
            RequiresAdvanceOrder: requiresAdvanceOrder,
            MinimumAdvanceOrderQuantity: minimumAdvanceOrderQuantity,
            IsAlwaysAvailable: isAlwaysAvailable,
            AvailableDays: availableDays ?? [],
            AllergenNotes: allergenNotes,
            PriceOptions: priceOptions);

        return _create.Execute(command);
    }

    #region Success Tests

    [Fact]
    public void Execute_WithValidCommand_UpdatesMenuItem()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: "Pulpo a la Gallega",
            Description: "Nueva descripción",
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var result = _update.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.Name.Should().Be("Pulpo a la Gallega");
        result.Description.Should().Be("Nueva descripción");
    }

    [Fact]
    public void Execute_WithAllFields_UpdatesAllFields()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: "Updated Name",
            Description: "Updated Description",
            ImageUrl: "https://example.com/updated.jpg",
            DisplayOrder: 10,
            IsHighRiskItem: true,
            RequiresAdvanceOrder: true,
            MinimumAdvanceOrderQuantity: 5,
            IsAlwaysAvailable: false,
            AvailableDays: [DayOfWeek.Monday, DayOfWeek.Wednesday],
            AllergenNotes: "Contains allergens");

        var result = _update.Execute(menuItem, command);

        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.ImageUrl.Should().Be("https://example.com/updated.jpg");
        result.DisplayOrder.Should().Be(10);
        result.IsHighRiskItem.Should().BeTrue();
        result.RequiresAdvanceOrder.Should().BeTrue();
        result.MinimumAdvanceOrderQuantity.Should().Be(5);
        result.IsAlwaysAvailable.Should().BeFalse();
        result.AvailableDays.Should().BeEquivalentTo([DayOfWeek.Monday, DayOfWeek.Wednesday]);
        result.AllergenNotes.Should().Be("Contains allergens");
    }

    [Fact]
    public void Execute_ChangingFromAlwaysAvailableToSpecificDays_UpdatesAvailableDays()
    {
        var menuItem = CreateMenuItem(isAlwaysAvailable: true);
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: false,
            AvailableDays: [DayOfWeek.Saturday],
            AllergenNotes: null);

        var result = _update.Execute(menuItem, command);

        result.IsAlwaysAvailable.Should().BeFalse();
        result.AvailableDays.Should().BeEquivalentTo([DayOfWeek.Saturday]);
    }

    [Fact]
    public void Execute_ChangingFromSpecificDaysToAlwaysAvailable_ClearsAvailableDays()
    {
        var menuItem = CreateMenuItem(
            isAlwaysAvailable: false,
            availableDays: [DayOfWeek.Monday, DayOfWeek.Tuesday]);
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var result = _update.Execute(menuItem, command);

        result.IsAlwaysAvailable.Should().BeTrue();
        result.AvailableDays.Should().BeEmpty();
    }

    [Fact]
    public void Execute_ActivatingHighRiskAndAdvanceOrder_UpdatesCorrectly()
    {
        var menuItem = CreateMenuItem(isHighRiskItem: false);
        var command = new UpdateMenuItemCommand(
            Name: "High Risk Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: true,
            RequiresAdvanceOrder: true,
            MinimumAdvanceOrderQuantity: 4,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var result = _update.Execute(menuItem, command);

        result.IsHighRiskItem.Should().BeTrue();
        result.RequiresAdvanceOrder.Should().BeTrue();
        result.MinimumAdvanceOrderQuantity.Should().Be(4);
    }

    [Fact]
    public void Execute_DeactivatingAdvanceOrderBeforeRemovingHighRisk_UpdatesCorrectly()
    {
        var menuItem = CreateMenuItem(
            isHighRiskItem: true,
            requiresAdvanceOrder: true,
            minimumAdvanceOrderQuantity: 4);
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: true,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var result = _update.Execute(menuItem, command);

        result.RequiresAdvanceOrder.Should().BeFalse();
        result.MinimumAdvanceOrderQuantity.Should().BeNull();
    }

    [Fact]
    public void Execute_ClearingOptionalFields_SetsToNull()
    {
        var menuItem = CreateMenuItem(
            description: "Original description",
            imageUrl: "https://example.com/original.jpg",
            allergenNotes: "Original notes");
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var result = _update.Execute(menuItem, command);

        result.Description.Should().BeNull();
        result.ImageUrl.Should().BeNull();
        result.AllergenNotes.Should().BeNull();
    }

    [Fact]
    public void Execute_PreservesIsActiveAndIsAvailable()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: "Updated Name",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var result = _update.Execute(menuItem, command);

        result.IsActive.Should().BeFalse();
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Execute_PreservesPriceOptions()
    {
        var menuItem = CreateMenuItem();
        var originalPriceOptionsCount = menuItem.PriceOptions.Count;
        var command = new UpdateMenuItemCommand(
            Name: "Updated Name",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var result = _update.Execute(menuItem, command);

        result.PriceOptions.Should().HaveCount(originalPriceOptionsCount);
    }

    #endregion

    #region Validation Throws

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: name!,
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var act = () => _update.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: new string('a', 101),
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var act = () => _update.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: new string('a', 1001),
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var act = () => _update.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithInvalidImageUrl_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: "not-a-valid-url",
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var act = () => _update.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: -1,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var act = () => _update.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithRequiresAdvanceOrderButNotHighRisk_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: true,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var act = () => _update.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_RemovingHighRiskWithAdvanceOrderActive_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem(
            isHighRiskItem: true,
            requiresAdvanceOrder: true);
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: true,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var act = () => _update.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Execute_WithInvalidMinimumAdvanceOrderQuantity_ThrowsValidationException(int quantity)
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: true,
            RequiresAdvanceOrder: true,
            MinimumAdvanceOrderQuantity: quantity,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var act = () => _update.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithMinimumQuantityButNoAdvanceOrder_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: 5,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null);

        var act = () => _update.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNotAlwaysAvailableAndNoAvailableDays_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: false,
            AvailableDays: [],
            AllergenNotes: null);

        var act = () => _update.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithAllergenNotesExceedingMaxLength_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = new UpdateMenuItemCommand(
            Name: "Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: new string('a', 501));

        var act = () => _update.Execute(menuItem, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
