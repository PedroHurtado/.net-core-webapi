namespace Auth.UnitTests.Infrastructure.Google;

public class GoogleIdTokenClaimsTests
{
    [Fact]
    public void Record_ShouldHaveExpectedProperties()
    {
        var claims = new GoogleIdTokenClaims(
            Sub: "google|123",
            Email: "pedro@test.com",
            Name: "Pedro",
            Picture: "https://photo.jpg");

        claims.Sub.Should().Be("google|123");
        claims.Email.Should().Be("pedro@test.com");
        claims.Name.Should().Be("Pedro");
        claims.Picture.Should().Be("https://photo.jpg");
    }

    [Fact]
    public void Record_WithNullPicture_ShouldHaveNullPicture()
    {
        var claims = new GoogleIdTokenClaims(
            Sub: "google|123",
            Email: "pedro@test.com",
            Name: "Pedro",
            Picture: null);

        claims.Picture.Should().BeNull();
    }
}
