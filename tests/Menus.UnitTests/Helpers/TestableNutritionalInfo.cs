namespace Menus.UnitTests.Helpers;

public record TestableNutritionalInfo : NutritionalInfo
{
    public TestableNutritionalInfo(
        int calories,
        decimal protein,
        decimal carbohydrates,
        decimal fat,
        int servingSize,
        decimal? fiber = null,
        decimal? sugar = null,
        decimal? salt = null)
        : base(calories, protein, carbohydrates, fat, servingSize, fiber, sugar, salt) { }
}
