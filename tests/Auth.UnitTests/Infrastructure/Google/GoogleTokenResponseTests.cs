namespace Auth.UnitTests.Infrastructure.Google;

public class GoogleTokenResponseTests
{
    [Fact]
    public void Record_ShouldHaveExpectedProperties()
    {
        var response = new GoogleTokenResponse(
            IdToken: "id-token-jwt",
            AccessToken: "access-token",
            TokenType: "Bearer",
            ExpiresIn: 3600);

        response.IdToken.Should().Be("id-token-jwt");
        response.AccessToken.Should().Be("access-token");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
    }
}
