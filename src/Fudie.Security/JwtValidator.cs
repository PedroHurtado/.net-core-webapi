namespace Fudie.Security;

public class JwtValidator(IJwksApi jwksApi, IOptions<FudieSecurityOptions> options) : IJwtValidator
{
    private readonly object _lock = new();
    private ECDsaSecurityKey? _cachedKey;
    private string? _cachedKid;
    private DateTime _cacheExpiry = DateTime.MinValue;

    public async Task<FudieTokenContext?> ValidateTokenAsync(string token)
    {
        var key = await GetSigningKeyAsync();
        if (key is null) return null;

        var handler = new JsonWebTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromSeconds(10)
        };

        var result = await handler.ValidateTokenAsync(token, validationParameters);
        if (!result.IsValid) return null;

        return ExtractContext(result);
    }

    private static FudieTokenContext ExtractContext(TokenValidationResult result)
    {
        var claims = result.Claims;

        var userId = claims.TryGetValue("sub", out var sub) && Guid.TryParse(sub?.ToString(), out var uid)
            ? uid
            : Guid.Empty;

        Guid? tenantId = claims.TryGetValue("tid", out var tid) && Guid.TryParse(tid?.ToString(), out var tId)
            ? tId
            : null;

        var isOwner = claims.TryGetValue("owner", out var owner) && owner is true;

        var groups = ExtractStringArray(claims, "groups");
        var additionalScopes = ExtractStringArray(claims, "add");
        var excludedScopes = ExtractStringArray(claims, "exc");

        return new FudieTokenContext(userId, tenantId, isOwner, groups, additionalScopes, excludedScopes);
    }

    private static string[] ExtractStringArray(IDictionary<string, object> claims, string key)
    {
        if (!claims.TryGetValue(key, out var value)) return [];

        return value switch
        {
            string s => [s],
            string[] arr => arr,
            List<object> list => list.Select(x => x.ToString()!).ToArray(),
            System.Text.Json.JsonElement jsonElement when jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array
                => jsonElement.EnumerateArray().Select(x => x.GetString()!).ToArray(),
            _ => []
        };
    }

    private async Task<ECDsaSecurityKey?> GetSigningKeyAsync()
    {
        lock (_lock)
        {
            if (_cachedKey is not null && DateTime.UtcNow < _cacheExpiry)
                return _cachedKey;
        }

        var jwks = await jwksApi.GetJwksAsync();
        var jwk = jwks.Keys.FirstOrDefault();
        if (jwk is null) return null;

        var ecdsa = ECDsa.Create(new ECParameters
        {
            Curve = GetCurve(jwk.Crv),
            Q = new ECPoint
            {
                X = Base64UrlEncoder.DecodeBytes(jwk.X),
                Y = Base64UrlEncoder.DecodeBytes(jwk.Y)
            }
        });

        var key = new ECDsaSecurityKey(ecdsa) { KeyId = jwk.Kid };

        lock (_lock)
        {
            _cachedKey = key;
            _cachedKid = jwk.Kid;
            _cacheExpiry = DateTime.UtcNow.AddMinutes(options.Value.CacheRefreshMinutes);
        }

        return key;
    }

    private static ECCurve GetCurve(string crv) => crv switch
    {
        "P-256" => ECCurve.NamedCurves.nistP256,
        "P-384" => ECCurve.NamedCurves.nistP384,
        "P-521" => ECCurve.NamedCurves.nistP521,
        _ => throw new NotSupportedException($"Unsupported curve: {crv}")
    };
}
