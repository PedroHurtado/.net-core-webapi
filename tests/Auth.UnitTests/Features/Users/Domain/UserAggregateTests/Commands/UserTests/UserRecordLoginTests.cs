namespace Auth.UnitTests.Users.Domain.UserAggregateTests.Commands.UserTests;

public class UserRecordLoginTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly User.RecordLogin _recordLogin = fixture.Get<User.RecordLogin>();

    [Fact]
    public void Execute_WithValidUser_UpdatesLastLoginAt()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("pedro@test.com")
            .WithName("Pedro")
            .WithIsActive(true);

        var result = _recordLogin.Execute(user);

        result.LastLoginAt.Should().NotBeNull();
        result.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
