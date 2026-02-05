namespace Menus.UnitTests.Features.Menus.Domain.MenuAggregate;

public class MenuTests
{
    [Fact]
    public void ParameterlessConstructor_InitializesWithEmptyId()
    {
        var menu = new TestableMenu();

        menu.Id.Should().BeEmpty();
    }

    [Fact]
    public void Id_SetsCorrectValue()
    {
        var id = Guid.NewGuid();
        var menu = new TestableMenu(id);

        menu.Id.Should().Be(id);
    }

    [Fact]
    public void TenantId_SetsCorrectValue()
    {
        var tenantId = Guid.NewGuid();
        var menu = new TestableMenu(Guid.NewGuid()) { TenantId = tenantId };

        menu.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void Name_SetsCorrectValue()
    {
        var menu = new TestableMenu(Guid.NewGuid()) { Name = "Lunch Menu" };

        menu.Name.Should().Be("Lunch Menu");
    }

    [Fact]
    public void Description_SetsCorrectValue()
    {
        var menu = new TestableMenu(Guid.NewGuid()) { Description = "Daily lunch specials" };

        menu.Description.Should().Be("Daily lunch specials");
    }

    [Fact]
    public void IsActive_SetsCorrectValue()
    {
        var menu = new TestableMenu(Guid.NewGuid()) { IsActive = true };

        menu.IsActive.Should().BeTrue();
    }

    [Fact]
    public void DisplayOrder_SetsCorrectValue()
    {
        var menu = new TestableMenu(Guid.NewGuid()) { DisplayOrder = 5 };

        menu.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void EffectiveFrom_SetsCorrectValue()
    {
        var date = new DateTime(2024, 1, 1);
        var menu = new TestableMenu(Guid.NewGuid()) { EffectiveFrom = date };

        menu.EffectiveFrom.Should().Be(date);
    }

    [Fact]
    public void EffectiveUntil_SetsCorrectValue()
    {
        var date = new DateTime(2024, 12, 31);
        var menu = new TestableMenu(Guid.NewGuid()) { EffectiveUntil = date };

        menu.EffectiveUntil.Should().Be(date);
    }

    [Fact]
    public void DepositPolicy_SetsCorrectValue()
    {
        var depositPolicy = new TestableDepositPolicy(DepositType.FixedAmount, 50m);
        var menu = new TestableMenu(Guid.NewGuid()) { DepositPolicy = depositPolicy };

        menu.DepositPolicy.Should().Be(depositPolicy);
    }

    [Fact]
    public void Categories_InitializesAsEmptyCollection()
    {
        var menu = new TestableMenu(Guid.NewGuid());

        menu.Categories.Should().BeEmpty();
    }
}
