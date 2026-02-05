namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.NutritionalInfo;

public class NutritionalInfoCreateTests
{
    private readonly NutritionalInfoValidator _validator = new();
    private readonly NutritionalInfoVO.Create _create;

    public NutritionalInfoCreateTests()
    {
        _create = new(_validator);
    }

    [Fact]
    public void Execute_WithValidCommand_ReturnsNutritionalInfo()
    {
        var command = new CreateNutritionalInfoCommand(
            Calories: 500,
            Protein: 25.0m,
            Carbohydrates: 60.0m,
            Fat: 15.0m,
            ServingSize: 350);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Calories.Should().Be(500);
        result.Protein.Should().Be(25.0m);
        result.Carbohydrates.Should().Be(60.0m);
        result.Fat.Should().Be(15.0m);
        result.ServingSize.Should().Be(350);
    }

    [Fact]
    public void Execute_WithOptionalFields_SetsOptionalFields()
    {
        var command = new CreateNutritionalInfoCommand(
            Calories: 500,
            Protein: 25.0m,
            Carbohydrates: 60.0m,
            Fat: 15.0m,
            ServingSize: 350,
            Fiber: 5.0m,
            Sugar: 10.0m,
            Salt: 2.0m);

        var result = _create.Execute(command);

        result.Fiber.Should().Be(5.0m);
        result.Sugar.Should().Be(10.0m);
        result.Salt.Should().Be(2.0m);
    }

    [Fact]
    public void Execute_WithoutOptionalFields_DefaultsToNull()
    {
        var command = new CreateNutritionalInfoCommand(
            Calories: 500,
            Protein: 25.0m,
            Carbohydrates: 60.0m,
            Fat: 15.0m,
            ServingSize: 350);

        var result = _create.Execute(command);

        result.Fiber.Should().BeNull();
        result.Sugar.Should().BeNull();
        result.Salt.Should().BeNull();
    }

    #region Validation Throws

    [Theory]
    [InlineData(-1)]
    [InlineData(10001)]
    public void Execute_WithInvalidCalories_ThrowsValidationException(int calories)
    {
        var command = new CreateNutritionalInfoCommand(calories, 25.0m, 60.0m, 15.0m, 350);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Execute_WithInvalidProtein_ThrowsValidationException(int protein)
    {
        var command = new CreateNutritionalInfoCommand(500, protein, 60.0m, 15.0m, 350);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Execute_WithInvalidCarbohydrates_ThrowsValidationException(int carbohydrates)
    {
        var command = new CreateNutritionalInfoCommand(500, 25.0m, carbohydrates, 15.0m, 350);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Execute_WithInvalidFat_ThrowsValidationException(int fat)
    {
        var command = new CreateNutritionalInfoCommand(500, 25.0m, 60.0m, fat, 350);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10001)]
    public void Execute_WithInvalidServingSize_ThrowsValidationException(int servingSize)
    {
        var command = new CreateNutritionalInfoCommand(500, 25.0m, 60.0m, 15.0m, servingSize);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Execute_WithInvalidFiber_ThrowsValidationException(int fiber)
    {
        var command = new CreateNutritionalInfoCommand(500, 25.0m, 60.0m, 15.0m, 350, Fiber: fiber);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Execute_WithInvalidSugar_ThrowsValidationException(int sugar)
    {
        var command = new CreateNutritionalInfoCommand(500, 25.0m, 60.0m, 15.0m, 350, Sugar: sugar);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Execute_WithInvalidSalt_ThrowsValidationException(int salt)
    {
        var command = new CreateNutritionalInfoCommand(500, 25.0m, 60.0m, 15.0m, 350, Salt: salt);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}