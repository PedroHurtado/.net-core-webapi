namespace Customer.UnitTests.Features.Menus.Domain.Shared.Enums;

public class PortionTypeTests
{
    [Theory]
    [InlineData(PortionType.Small, "Small")]
    [InlineData(PortionType.Half, "Half")]
    [InlineData(PortionType.Full, "Full")]
    [InlineData(PortionType.MarketPrice, "MarketPrice")]
    public void ToString_ReturnsExpectedStringName(PortionType portionType, string expectedName)
    {
        portionType.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(PortionType.Small, 1)]
    [InlineData(PortionType.Half, 2)]
    [InlineData(PortionType.Full, 3)]
    [InlineData(PortionType.MarketPrice, 4)]
    public void Value_ReturnsExpectedInteger(PortionType portionType, int expectedValue)
    {
        ((int)portionType).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_HasExpectedMemberCount()
    {
        Enum.GetValues<PortionType>().Should().HaveCount(4);
    }
}