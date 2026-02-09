namespace Auth.UnitTests.Infrastructure.Google;

public class GoogleOAuthUrlTests
{
    [Fact]
    public void Record_ShouldHaveExpectedProperties()
    {
        var result = new GoogleOAuthUrl(
            Url: "https://accounts.google.com/o/oauth2/auth?client_id=123",
            State: "random-state");

        result.Url.Should().Be("https://accounts.google.com/o/oauth2/auth?client_id=123");
        result.State.Should().Be("random-state");
    }
}
