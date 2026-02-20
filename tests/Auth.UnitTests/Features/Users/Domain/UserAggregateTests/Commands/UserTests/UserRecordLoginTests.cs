namespace Auth.UnitTests.Users.Domain.UserAggregateTests.Commands.UserTests;

public class UserRecordLoginTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly User.RecordLogin _recordLogin = fixture.Get<User.RecordLogin>();

    [Fact]
    public void Execute_WithValidUser_UpdatesLastLoginAt()
    {
        var now = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("pedro@test.com")
            .WithName("Pedro")
            .WithIsActive(true);

        var result = _recordLogin.Execute(user, new RecordLoginCommand(Now: now));

        result.LastLoginAt.Should().Be(now);
    }
}
