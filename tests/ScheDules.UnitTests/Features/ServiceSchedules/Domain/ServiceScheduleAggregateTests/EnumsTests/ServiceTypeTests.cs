namespace Schedules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.EnumsTests;

public class ServiceTypeTests
{
    [Theory]
    [InlineData(ServiceType.Breakfast, "Breakfast")]
    [InlineData(ServiceType.Lunch, "Lunch")]
    [InlineData(ServiceType.Dinner, "Dinner")]
    public void ToString_ReturnsExpectedStringName(ServiceType value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(ServiceType.Breakfast, 1)]
    [InlineData(ServiceType.Lunch, 2)]
    [InlineData(ServiceType.Dinner, 3)]
    public void Value_ReturnsExpectedInteger(ServiceType value, int expectedValue)
    {
        ((int)value).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_HasExpectedMemberCount()
    {
        Enum.GetValues<ServiceType>().Should().HaveCount(3);
    }
}
