namespace Menus.UnitTests.Features.Menus.Domain.MenuAggregate.Entities;

public class MenuCategoryTests
{
    [Fact]
    public void ParameterlessConstructor_InitializesWithEmptyId()
    {
        var category = new TestableMenuCategory();

        category.Id.Should().BeEmpty();
    }

    [Fact]
    public void Id_SetsCorrectValue()
    {
        var id = Guid.NewGuid();
        var category = new TestableMenuCategory(id);

        category.Id.Should().Be(id);
    }

    [Fact]
    public void Name_SetsCorrectValue()
    {
        var category = new TestableMenuCategory(Guid.NewGuid()) { Name = "Appetizers" };

        category.Name.Should().Be("Appetizers");
    }

    [Fact]
    public void Description_SetsCorrectValue()
    {
        var category = new TestableMenuCategory(Guid.NewGuid()) { Description = "Starters and snacks" };

        category.Description.Should().Be("Starters and snacks");
    }

    [Fact]
    public void DisplayOrder_SetsCorrectValue()
    {
        var category = new TestableMenuCategory(Guid.NewGuid()) { DisplayOrder = 5 };

        category.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void IsActive_SetsCorrectValue()
    {
        var category = new TestableMenuCategory(Guid.NewGuid()) { IsActive = true };

        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Items_InitializesAsEmptyCollection()
    {
        var category = new TestableMenuCategory(Guid.NewGuid());

        category.Items.Should().BeEmpty();
    }
}
