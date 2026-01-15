namespace Customer.UnitTests.Features.Menus.Domain.AllergenAggregate;

public class AllergenTests
{
    [Fact]
    public void ParameterlessConstructor_InitializesWithEmptyId()
    {
        var allergen = new TestableAllergen();

        allergen.Id.Should().BeEmpty();
    }

    [Fact]
    public void Id_SetsCorrectValue()
    {
        var allergen = new TestableAllergen("GLUTEN");

        allergen.Id.Should().Be("GLUTEN");
    }

    [Fact]
    public void Name_SetsCorrectValue()
    {
        var allergen = new TestableAllergen("GLUTEN") { Name = "Gluten" };

        allergen.Name.Should().Be("Gluten");
    }

    [Fact]
    public void IconUrl_SetsCorrectValue()
    {
        var allergen = new TestableAllergen("GLUTEN") { IconUrl = "https://example.com/icon.png" };

        allergen.IconUrl.Should().Be("https://example.com/icon.png");
    }

    [Fact]
    public void IsActive_SetsCorrectValue()
    {
        var allergen = new TestableAllergen("GLUTEN") { IsActive = true };

        allergen.IsActive.Should().BeTrue();
    }

    [Fact]
    public void DisplayOrder_SetsCorrectValue()
    {
        var allergen = new TestableAllergen("GLUTEN") { DisplayOrder = 5 };

        allergen.DisplayOrder.Should().Be(5);
    }
}
