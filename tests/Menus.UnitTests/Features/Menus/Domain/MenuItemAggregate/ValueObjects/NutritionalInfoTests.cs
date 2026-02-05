namespace Menus.UnitTests.Features.Menus.Domain.MenuItemAggregate.ValueObjects;

public class NutritionalInfoTests
{
    [Fact]
    public void Constructor_SetsAllRequiredProperties()
    {
        var info = new TestableNutritionalInfo(
            calories: 500,
            protein: 25.5m,
            carbohydrates: 60.0m,
            fat: 15.0m,
            servingSize: 350);

        info.Calories.Should().Be(500);
        info.Protein.Should().Be(25.5m);
        info.Carbohydrates.Should().Be(60.0m);
        info.Fat.Should().Be(15.0m);
        info.ServingSize.Should().Be(350);
    }

    [Fact]
    public void Constructor_SetsOptionalProperties()
    {
        var info = new TestableNutritionalInfo(
            calories: 500,
            protein: 25.5m,
            carbohydrates: 60.0m,
            fat: 15.0m,
            servingSize: 350,
            fiber: 5.0m,
            sugar: 10.0m,
            salt: 2.5m);

        info.Fiber.Should().Be(5.0m);
        info.Sugar.Should().Be(10.0m);
        info.Salt.Should().Be(2.5m);
    }

    [Fact]
    public void Constructor_OptionalPropertiesDefaultToNull()
    {
        var info = new TestableNutritionalInfo(
            calories: 500,
            protein: 25.5m,
            carbohydrates: 60.0m,
            fat: 15.0m,
            servingSize: 350);

        info.Fiber.Should().BeNull();
        info.Sugar.Should().BeNull();
        info.Salt.Should().BeNull();
    }

    #region GetNutritionForPortion

    [Fact]
    public void GetNutritionForPortion_HalfPortion_ReturnsHalfValues()
    {
        var info = new TestableNutritionalInfo(
            calories: 500,
            protein: 20.0m,
            carbohydrates: 60.0m,
            fat: 10.0m,
            servingSize: 300,
            fiber: 4.0m,
            sugar: 8.0m,
            salt: 2.0m);

        var half = info.GetNutritionForPortion(0.5m);

        half.Calories.Should().Be(250);
        half.Protein.Should().Be(10.0m);
        half.Carbohydrates.Should().Be(30.0m);
        half.Fat.Should().Be(5.0m);
        half.ServingSize.Should().Be(150);
        half.Fiber.Should().Be(2.0m);
        half.Sugar.Should().Be(4.0m);
        half.Salt.Should().Be(1.0m);
    }

    [Fact]
    public void GetNutritionForPortion_SmallPortion_ReturnsQuarterValues()
    {
        var info = new TestableNutritionalInfo(
            calories: 400,
            protein: 20.0m,
            carbohydrates: 40.0m,
            fat: 8.0m,
            servingSize: 200);

        var small = info.GetNutritionForPortion(0.25m);

        small.Calories.Should().Be(100);
        small.Protein.Should().Be(5.0m);
        small.Carbohydrates.Should().Be(10.0m);
        small.Fat.Should().Be(2.0m);
        small.ServingSize.Should().Be(50);
    }

    [Fact]
    public void GetNutritionForPortion_WithNullOptionals_PreservesNull()
    {
        var info = new TestableNutritionalInfo(
            calories: 400,
            protein: 20.0m,
            carbohydrates: 40.0m,
            fat: 8.0m,
            servingSize: 200);

        var half = info.GetNutritionForPortion(0.5m);

        half.Fiber.Should().BeNull();
        half.Sugar.Should().BeNull();
        half.Salt.Should().BeNull();
    }

    #endregion
}
