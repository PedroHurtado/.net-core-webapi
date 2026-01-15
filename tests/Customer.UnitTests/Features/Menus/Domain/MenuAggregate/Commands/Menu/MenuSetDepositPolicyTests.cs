namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuSetDepositPolicyTests
{
    private readonly MenuValidator _menuValidator = new();
    private readonly DepositPolicyValidator _depositPolicyValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.SetDepositPolicy _setDepositPolicy;

    public MenuSetDepositPolicyTests()
    {
        _createMenu = new(_menuValidator);
        var createDepositPolicy = new DepositPolicyVO.Create(_depositPolicyValidator);
        _setDepositPolicy = new(_menuValidator, createDepositPolicy);
    }

    private MenuAgg CreateValidMenu() => _createMenu.Execute(new CreateMenuCommand(
        TenantId: Guid.NewGuid(),
        Name: "Test Menu"
    ));

    [Fact]
    public void Execute_WithValidCommand_SetsDepositPolicy()
    {
        var menu = CreateValidMenu();
        var command = new SetDepositPolicyCommand(
            DepositType: DepositType.PerPerson,
            Amount: 25.00m);

        var result = _setDepositPolicy.Execute(menu, command);

        result.DepositPolicy.Should().NotBeNull();
        result.DepositPolicy!.DepositType.Should().Be(DepositType.PerPerson);
        result.DepositPolicy.Amount.Should().Be(25.00m);
    }

    [Fact]
    public void Execute_WithExistingPolicy_ReplacesPolicy()
    {
        var menu = CreateValidMenu();
        _setDepositPolicy.Execute(menu, new SetDepositPolicyCommand(
            DepositType: DepositType.FixedAmount,
            Amount: 50.00m));
        var newCommand = new SetDepositPolicyCommand(
            DepositType: DepositType.PerPerson,
            Amount: 35.00m);

        var result = _setDepositPolicy.Execute(menu, newCommand);

        result.DepositPolicy!.DepositType.Should().Be(DepositType.PerPerson);
        result.DepositPolicy.Amount.Should().Be(35.00m);
    }
}