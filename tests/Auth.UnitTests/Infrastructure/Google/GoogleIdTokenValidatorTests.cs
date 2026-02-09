namespace Auth.UnitTests.Infrastructure.Google;

public class GoogleIdTokenValidatorTests
{
    private static (string idToken, string certPem) CreateSignedToken(
        string sub = "google|123",
        string email = "pedro@test.com",
        string name = "Pedro",
        string? picture = "https://photo.jpg",
        string audience = "test-client-id",
        string issuer = "https://accounts.google.com",
        RSA? signingKey = null)
    {
        var rsa = signingKey ?? RSA.Create(2048);
        var certRequest = new CertificateRequest(
            "cn=test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var cert = certRequest.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddYears(1));
        var pem = cert.ExportCertificatePem();

        var securityKey = new RsaSecurityKey(rsa);
        var handler = new JsonWebTokenHandler();

        var claims = new Dictionary<string, object>
        {
            ["sub"] = sub,
            ["email"] = email,
            ["name"] = name
        };

        if (picture is not null)
            claims["picture"] = picture;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256),
            Expires = DateTime.UtcNow.AddHours(1)
        };

        var idToken = handler.CreateToken(tokenDescriptor);
        return (idToken, pem);
    }

    private static GoogleIdTokenValidator CreateValidator(string certPem, string clientId = "test-client-id")
    {
        var settings = new GoogleOAuthSettings(
            ClientId: clientId,
            ClientSecret: "test-secret",
            RedirectUri: "http://localhost/callback",
            AuthUri: "https://accounts.google.com/o/oauth2/auth",
            TokenUri: "https://oauth2.googleapis.com/token",
            CertsUri: "https://certs.example.com");

        var mockSettings = new Mock<IGoogleOAuthSettings>();
        mockSettings.Setup(s => s.Get()).Returns(settings);

        var certsJson = JsonSerializer.Serialize(
            new Dictionary<string, string> { ["kid1"] = certPem });

        var mockHandler = new FakeHttpMessageHandler(certsJson);
        var httpClient = new HttpClient(mockHandler);

        return new GoogleIdTokenValidator(mockSettings.Object, httpClient);
    }

    [Fact]
    public async Task ValidateAsync_WithValidToken_ReturnsClaims()
    {
        var (idToken, certPem) = CreateSignedToken();
        var validator = CreateValidator(certPem);

        var result = await validator.ValidateAsync(idToken);

        result.Sub.Should().Be("google|123");
        result.Email.Should().Be("pedro@test.com");
        result.Name.Should().Be("Pedro");
        result.Picture.Should().Be("https://photo.jpg");
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidSignature_ThrowsException()
    {
        var differentRsa = RSA.Create(2048);
        var (idToken, _) = CreateSignedToken();
        var (_, certPem) = CreateSignedToken(signingKey: differentRsa);

        var validator = CreateValidator(certPem);

        var act = () => validator.ValidateAsync(idToken);

        await act.Should().ThrowAsync<SecurityTokenValidationException>();
    }

    private class FakeHttpMessageHandler(string responseContent) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            });
        }
    }
}
