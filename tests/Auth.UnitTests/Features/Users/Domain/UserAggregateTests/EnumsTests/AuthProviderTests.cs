namespace Auth.UnitTests.Features.Users.Domain.UserAggregateTests.EnumsTests;

public class AuthProviderTests
{
    [Theory]
    [InlineData(AuthProvider.Google, "Google")]
    [InlineData(AuthProvider.Local, "Local")]
    public void ToString_ReturnsExpectedStringName(AuthProvider value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(AuthProvider.Google, 1)]
    [InlineData(AuthProvider.Local, 2)]
    public void Value_ReturnsExpectedInteger(AuthProvider value, int expectedValue)
    {
        ((int)value).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_HasExpectedMemberCount()
    {
        Enum.GetValues<AuthProvider>().Should().HaveCount(2);
    }
}
