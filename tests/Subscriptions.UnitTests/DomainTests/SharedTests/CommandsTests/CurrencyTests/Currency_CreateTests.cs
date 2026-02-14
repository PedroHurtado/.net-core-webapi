namespace Subscriptions.UnitTests.DomainTests.SharedTests.CommandsTests.CurrencyTests;

public class CurrencyCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Currency.Create _create = fixture.Get<Currency.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsCurrency()
    {
        var command = new CreateCurrencyCommand("EUR");

        var result = _create.Execute(command);

        result.Code.Should().Be("EUR");
        result.Symbol.Should().Be("€");
        result.DecimalPlaces.Should().Be(2);
    }
}
