namespace Auth.UnitTests.Features.Shared.EnumsTests;

public class InvitationStatusTests
{
    [Theory]
    [InlineData(InvitationStatus.Pending, "Pending")]
    [InlineData(InvitationStatus.Accepted, "Accepted")]
    [InlineData(InvitationStatus.Cancelled, "Cancelled")]
    public void ToString_ReturnsExpectedStringName(InvitationStatus value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(InvitationStatus.Pending, 1)]
    [InlineData(InvitationStatus.Accepted, 2)]
    [InlineData(InvitationStatus.Cancelled, 3)]
    public void Value_ReturnsExpectedInteger(InvitationStatus value, int expectedValue)
    {
        ((int)value).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_HasExpectedMemberCount()
    {
        Enum.GetValues<InvitationStatus>().Should().HaveCount(3);
    }
}
