namespace Auth.UnitTests.Users.Domain.UserAggregateTests.Commands.UserTests;

public class UserUpdateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly User.Update _update = fixture.Get<User.Update>();

    [Fact]
    public void Execute_WithValidCommand_UpdatesUser()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("pedro@test.com")
            .WithName("Old Name")
            .WithIsActive(true);

        var command = new UpdateUserCommand(
            Name: "New Name",
            Phone: "+34666999888");

        var result = _update.Execute(user, command);

        result.Name.Should().Be("New Name");
        result.Phone.Should().Be("+34666999888");
    }
}
