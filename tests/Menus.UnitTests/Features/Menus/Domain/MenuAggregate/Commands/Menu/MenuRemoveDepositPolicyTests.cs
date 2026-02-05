namespace Menus.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuRemoveDepositPolicyTests
{
    private readonly MenuValidator _menuValidator = new();
    private readonly DepositPolicyValidator _depositPolicyValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.SetDepositPolicy _setDepositPolicy;
    private readonly MenuAgg.RemoveDepositPolicy _removeDepositPolicy;

    public MenuRemoveDepositPolicyTests()
    {
        _createMenu = new(_menuValidator);
        var createDepositPolicy = new DepositPolicyVO.Create(_depositPolicyValidator);
        _setDepositPolicy = new(_menuValidator, createDepositPolicy);
        _removeDepositPolicy = new(_menuValidator);
    }

    private MenuAgg CreateMenuWithDepositPolicy()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Menu"
        ));
        _setDepositPolicy.Execute(menu, new SetDepositPolicyCommand(
            DepositType: DepositType.PerPerson,
            Amount: 25.00m));
        return menu;
    }

    [Fact]
    public void Execute_WithExistingPolicy_RemovesPolicy()
    {
        var menu = CreateMenuWithDepositPolicy();
        menu.DepositPolicy.Should().NotBeNull();

        var result = _removeDepositPolicy.Execute(menu);

        result.DepositPolicy.Should().BeNull();
    }

    [Fact]
    public void Execute_WithNoPolicy_RemainsNull()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Menu"
        ));

        var result = _removeDepositPolicy.Execute(menu);

        result.DepositPolicy.Should().BeNull();
    }
}