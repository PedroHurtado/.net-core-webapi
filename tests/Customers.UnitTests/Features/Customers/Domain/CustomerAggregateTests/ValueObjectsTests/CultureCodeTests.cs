namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class CultureCodeTests
{
    [Theory]
    [InlineData("es-ES")]
    [InlineData("en-GB")]
    public void Code_SetsCorrectValue(string code)
    {
        var cultureCode = new CultureCode(code);

        cultureCode.Code.Should().Be(code);
    }
}
