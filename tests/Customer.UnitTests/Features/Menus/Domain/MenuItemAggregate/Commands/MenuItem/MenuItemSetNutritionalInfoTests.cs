namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.MenuItem;

public class MenuItemSetNutritionalInfoTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly NutritionalInfoValidator _nutritionalInfoValidator = new();
    private readonly PriceOptionVO.Create _priceOptionCreate;
    private readonly NutritionalInfoVO.Create _nutritionalInfoCreate;
    private readonly MenuItemAgg.Create _create;
    private readonly MenuItemAgg.SetNutritionalInfo _setNutritionalInfo;

    public MenuItemSetNutritionalInfoTests()
    {
        _priceOptionCreate = new(_priceOptionValidator);
        _nutritionalInfoCreate = new(_nutritionalInfoValidator);
        _create = new(_priceOptionCreate, _validator);
        _setNutritionalInfo = new(_nutritionalInfoCreate, _validator);
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

    private static SetNutritionalInfoCommand CreateValidNutritionalInfoCommand(
        int calories = 500,
        decimal protein = 25.0m,
        decimal carbohydrates = 40.0m,
        decimal fat = 15.0m,
        int servingSize = 350,
        decimal? fiber = null,
        decimal? sugar = null,
        decimal? salt = null)
    {
        return new SetNutritionalInfoCommand(
            Calories: calories,
            Protein: protein,
            Carbohydrates: carbohydrates,
            Fat: fat,
            ServingSize: servingSize,
            Fiber: fiber,
            Sugar: sugar,
            Salt: salt
        );
    }

    #region Success Tests

    [Fact]
    public void Execute_WithValidData_SetsNutritionalInfo()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(
            calories: 500,
            protein: 25.0m,
            carbohydrates: 40.0m,
            fat: 15.0m,
            servingSize: 350
        );

        var result = _setNutritionalInfo.Execute(menuItem, command);

        result.Should().NotBeNull();
        result.NutritionalInfo.Should().NotBeNull();
        result.NutritionalInfo!.Calories.Should().Be(500);
        result.NutritionalInfo.Protein.Should().Be(25.0m);
        result.NutritionalInfo.Carbohydrates.Should().Be(40.0m);
        result.NutritionalInfo.Fat.Should().Be(15.0m);
        result.NutritionalInfo.ServingSize.Should().Be(350);
    }

    [Fact]
    public void Execute_WithOptionalFields_SetsAllFields()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(
            calories: 500,
            protein: 25.0m,
            carbohydrates: 40.0m,
            fat: 15.0m,
            servingSize: 350,
            fiber: 5.0m,
            sugar: 8.0m,
            salt: 1.5m
        );

        var result = _setNutritionalInfo.Execute(menuItem, command);

        result.NutritionalInfo!.Fiber.Should().Be(5.0m);
        result.NutritionalInfo.Sugar.Should().Be(8.0m);
        result.NutritionalInfo.Salt.Should().Be(1.5m);
    }

    [Fact]
    public void Execute_WithoutOptionalFields_LeavesOptionalFieldsNull()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(
            fiber: null,
            sugar: null,
            salt: null
        );

        var result = _setNutritionalInfo.Execute(menuItem, command);

        result.NutritionalInfo!.Fiber.Should().BeNull();
        result.NutritionalInfo.Sugar.Should().BeNull();
        result.NutritionalInfo.Salt.Should().BeNull();
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
        var command = CreateValidNutritionalInfoCommand();

        var result = _setNutritionalInfo.Execute(menuItem, command);

        result.Name.Should().Be("Test MenuItem");
        result.Description.Should().Be("Test Description");
        result.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Execute_PreservesPriceOptions()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand();

        var result = _setNutritionalInfo.Execute(menuItem, command);

        result.PriceOptions.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_ReplacesExistingNutritionalInfo()
    {
        var menuItem = CreateMenuItem();
        var firstCommand = CreateValidNutritionalInfoCommand(
            calories: 300,
            protein: 15.0m,
            carbohydrates: 30.0m,
            fat: 10.0m,
            servingSize: 250
        );
        var secondCommand = CreateValidNutritionalInfoCommand(
            calories: 600,
            protein: 35.0m,
            carbohydrates: 50.0m,
            fat: 20.0m,
            servingSize: 400
        );

        var firstResult = _setNutritionalInfo.Execute(menuItem, firstCommand);
        var secondResult = _setNutritionalInfo.Execute(firstResult, secondCommand);

        secondResult.NutritionalInfo!.Calories.Should().Be(600);
        secondResult.NutritionalInfo.Protein.Should().Be(35.0m);
        secondResult.NutritionalInfo.Carbohydrates.Should().Be(50.0m);
        secondResult.NutritionalInfo.Fat.Should().Be(20.0m);
        secondResult.NutritionalInfo.ServingSize.Should().Be(400);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Execute_WithNegativeCalories_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(calories: -1);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Calories cannot be negative")));
    }

    [Fact]
    public void Execute_WithCaloriesExceedingMax_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(calories: 10001);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Calories cannot exceed 10000")));
    }

    [Fact]
    public void Execute_WithNegativeProtein_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(protein: -1);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Protein cannot be negative")));
    }

    [Fact]
    public void Execute_WithProteinExceedingMax_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(protein: 1001);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Protein cannot exceed 1000g")));
    }

    [Fact]
    public void Execute_WithNegativeCarbohydrates_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(carbohydrates: -1);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Carbohydrates cannot be negative")));
    }

    [Fact]
    public void Execute_WithCarbohydratesExceedingMax_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(carbohydrates: 1001);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Carbohydrates cannot exceed 1000g")));
    }

    [Fact]
    public void Execute_WithNegativeFat_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(fat: -1);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Fat cannot be negative")));
    }

    [Fact]
    public void Execute_WithFatExceedingMax_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(fat: 1001);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Fat cannot exceed 1000g")));
    }

    [Fact]
    public void Execute_WithZeroServingSize_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(servingSize: 0);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Serving size must be greater than zero")));
    }

    [Fact]
    public void Execute_WithServingSizeExceedingMax_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(servingSize: 10001);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Serving size cannot exceed 10000g")));
    }

    [Fact]
    public void Execute_WithNegativeFiber_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(fiber: -1);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Fiber cannot be negative")));
    }

    [Fact]
    public void Execute_WithNegativeSugar_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(sugar: -1);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Sugar cannot be negative")));
    }

    [Fact]
    public void Execute_WithNegativeSalt_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(salt: -1);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Salt cannot be negative")));
    }

    [Fact]
    public void Execute_WithSaltExceedingMax_ThrowsValidationException()
    {
        var menuItem = CreateMenuItem();
        var command = CreateValidNutritionalInfoCommand(salt: 101);

        var act = () => _setNutritionalInfo.Execute(menuItem, command);

        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("Salt cannot exceed 100g")));
    }

    #endregion
}
