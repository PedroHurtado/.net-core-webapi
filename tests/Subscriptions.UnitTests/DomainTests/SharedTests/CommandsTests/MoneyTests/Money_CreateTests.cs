namespace Subscriptions.UnitTests.DomainTests.SharedTests.CommandsTests.MoneyTests;

public class MoneyCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Money.Create _create = fixture.Get<Money.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsMoney()
    {
        var command = new CreateMoneyCommand(9.99m, "EUR");

        var result = _create.Execute(command);

        result.Amount.Should().Be(9.99m);
        result.Currency.Code.Should().Be("EUR");
    }
}
