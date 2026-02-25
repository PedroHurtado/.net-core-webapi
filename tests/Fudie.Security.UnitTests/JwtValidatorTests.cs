using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Fudie.Security.UnitTests;

public class JwtValidatorTests : IDisposable
{
    private readonly ECDsa _ecdsa;
    private readonly ECDsaSecurityKey _privateKey;
    private readonly JwkEntry _jwkEntry;
    private readonly Mock<IJwksApi> _jwksApiMock;
    private readonly JwtValidator _validator;

    public JwtValidatorTests()
    {
        _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _privateKey = new ECDsaSecurityKey(_ecdsa) { KeyId = "test-kid-001" };

        var ecParams = _ecdsa.ExportParameters(false);
        _jwkEntry = new JwkEntry(
            Kty: "EC",
            Crv: "P-256",
            X: Base64UrlEncoder.Encode(ecParams.Q.X!),
            Y: Base64UrlEncoder.Encode(ecParams.Q.Y!),
            Kid: "test-kid-001",
            Use: "sig",
            Alg: "ES256");

        _jwksApiMock = new Mock<IJwksApi>();
        _jwksApiMock.Setup(x => x.GetJwksAsync())
            .ReturnsAsync(new JwksResponse([_jwkEntry]));

        var options = Options.Create(new FudieSecurityOptions
        {
            JwksUrl = "http://auth-service:8080/auth/jwks",
            CacheRefreshMinutes = 60
        });

        _validator = new JwtValidator(_jwksApiMock.Object, options);
    }

    public void Dispose() => _ecdsa.Dispose();

    #region Valid Token Tests

    [Fact]
    public async Task ValidateTokenAsync_WithValidSessionToken_ShouldReturnContext()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var token = CreateSessionToken(userId, tenantId, groups: ["menu:read", "menu:write"]);

        var result = await _validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.TenantId.Should().Be(tenantId);
        result.IsOwner.Should().BeFalse();
        result.Groups.Should().BeEquivalentTo(["menu:read", "menu:write"]);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithOwnerToken_ShouldReturnOwnerContext()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var token = CreateOwnerToken(userId, tenantId);

        var result = await _validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.TenantId.Should().Be(tenantId);
        result.IsOwner.Should().BeTrue();
        result.Groups.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithNoTenantToken_ShouldReturnNullTenantId()
    {
        var userId = Guid.NewGuid();
        var token = CreateTokenWithSubOnly(userId);

        var result = await _validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.TenantId.Should().BeNull();
        result.IsOwner.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithAdditionalAndExcludedScopes_ShouldExtractThem()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var token = CreateSessionToken(userId, tenantId,
            groups: ["menu:read"],
            additionalScopes: ["admin:manage"],
            excludedScopes: ["billing:delete"]);

        var result = await _validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.AdditionalScopes.Should().BeEquivalentTo(["admin:manage"]);
        result.ExcludedScopes.Should().BeEquivalentTo(["billing:delete"]);
    }

    #endregion

    #region Invalid Token Tests

    [Fact]
    public async Task ValidateTokenAsync_WithExpiredToken_ShouldReturnNull()
    {
        var token = CreateExpiredToken();

        var result = await _validator.ValidateTokenAsync(token);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidSignature_ShouldReturnNull()
    {
        var result = await _validator.ValidateTokenAsync("invalid.jwt.token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithDifferentKey_ShouldReturnNull()
    {
        using var otherKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var token = CreateTokenWithKey(otherKey);

        var result = await _validator.ValidateTokenAsync(token);

        result.Should().BeNull();
    }

    #endregion

    #region Caching Tests

    [Fact]
    public async Task ValidateTokenAsync_MultipleCalls_ShouldCacheJwks()
    {
        var token1 = CreateTokenWithSubOnly(Guid.NewGuid());
        var token2 = CreateTokenWithSubOnly(Guid.NewGuid());

        await _validator.ValidateTokenAsync(token1);
        await _validator.ValidateTokenAsync(token2);

        _jwksApiMock.Verify(x => x.GetJwksAsync(), Times.Once);
    }

    [Fact]
    public async Task ValidateTokenAsync_WhenJwksReturnsEmptyKeys_ShouldReturnNull()
    {
        _jwksApiMock.Setup(x => x.GetJwksAsync())
            .ReturnsAsync(new JwksResponse([]));

        var options = Options.Create(new FudieSecurityOptions
        {
            JwksUrl = "http://auth-service:8080/auth/jwks",
            CacheRefreshMinutes = 60
        });

        var validator = new JwtValidator(_jwksApiMock.Object, options);
        var token = CreateTokenWithSubOnly(Guid.NewGuid());

        var result = await validator.ValidateTokenAsync(token);

        result.Should().BeNull();
    }

    #endregion

    #region Token Helpers

    private string CreateSessionToken(
        Guid userId,
        Guid tenantId,
        string[]? groups = null,
        string[]? additionalScopes = null,
        string[]? excludedScopes = null)
    {
        var claims = new Dictionary<string, object>
        {
            ["sub"] = userId.ToString(),
            ["tid"] = tenantId.ToString(),
            ["groups"] = groups ?? [],
            ["add"] = additionalScopes ?? [],
            ["exc"] = excludedScopes ?? []
        };

        return CreateToken(claims);
    }

    private string CreateOwnerToken(Guid userId, Guid tenantId)
    {
        var claims = new Dictionary<string, object>
        {
            ["sub"] = userId.ToString(),
            ["tid"] = tenantId.ToString(),
            ["owner"] = true
        };

        return CreateToken(claims);
    }

    private string CreateTokenWithSubOnly(Guid userId)
    {
        var claims = new Dictionary<string, object>
        {
            ["sub"] = userId.ToString()
        };

        return CreateToken(claims);
    }

    private string CreateExpiredToken()
    {
        var claims = new Dictionary<string, object>
        {
            ["sub"] = Guid.NewGuid().ToString()
        };

        return CreateToken(claims, TimeSpan.FromSeconds(-60));
    }

    private string CreateTokenWithKey(ECDsa key)
    {
        var securityKey = new ECDsaSecurityKey(key) { KeyId = "other-kid" };
        var descriptor = new SecurityTokenDescriptor
        {
            Claims = new Dictionary<string, object> { ["sub"] = Guid.NewGuid().ToString() },
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private string CreateToken(Dictionary<string, object> claims, TimeSpan? lifetime = null)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Expires = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromMinutes(5)),
            SigningCredentials = new SigningCredentials(_privateKey, SecurityAlgorithms.EcdsaSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    #endregion
}
