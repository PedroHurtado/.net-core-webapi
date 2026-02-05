namespace Menus.UnitTests.Features.Menus.Domain.MenuAggregate.Enums;

public class DepositTypeTests
{
    [Theory]
    [InlineData(DepositType.PerPerson, "PerPerson")]
    [InlineData(DepositType.PercentageOfBill, "PercentageOfBill")]
    [InlineData(DepositType.FixedAmount, "FixedAmount")]
    public void ToString_ReturnsExpectedStringName(DepositType depositType, string expectedName)
    {
        depositType.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(DepositType.PerPerson, 1)]
    [InlineData(DepositType.PercentageOfBill, 2)]
    [InlineData(DepositType.FixedAmount, 3)]
    public void Value_ReturnsExpectedInteger(DepositType depositType, int expectedValue)
    {
        ((int)depositType).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_HasExpectedMemberCount()
    {
        Enum.GetValues<DepositType>().Should().HaveCount(3);
    }
}