namespace Auth.Infrastructure.Google;

public class GoogleIdTokenValidator(
    IGoogleOAuthSettings googleOAuthSettings,
    HttpClient httpClient
) : IGoogleIdTokenValidator
{
    private IList<SecurityKey>? _cachedKeys;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<GoogleIdTokenClaims> ValidateAsync(string idToken)
    {
        var settings = googleOAuthSettings.Get();
        var keys = await GetSigningKeysAsync(settings.CertsUri);

        var handler = new JsonWebTokenHandler();
        var parameters = new TokenValidationParameters
        {
            IssuerSigningKeys = keys,
            ValidAudience = settings.ClientId,
            ValidIssuers = ["https://accounts.google.com", "accounts.google.com"],
            ValidateLifetime = true
        };

        var result = await handler.ValidateTokenAsync(idToken, parameters);

        if (!result.IsValid)
            throw new SecurityTokenValidationException("Invalid Google id_token", result.Exception);

        return new GoogleIdTokenClaims(
            Sub: result.Claims["sub"].ToString()!,
            Email: result.Claims["email"].ToString()!,
            Name: result.Claims["name"].ToString()!,
            Picture: result.Claims.TryGetValue("picture", out var pic) ? pic?.ToString() : null
        );
    }

    private async Task<IList<SecurityKey>> GetSigningKeysAsync(string certsUri)
    {
        if (_cachedKeys is not null && DateTime.UtcNow < _cacheExpiry)
            return _cachedKeys;

        await _semaphore.WaitAsync();
        try
        {
            if (_cachedKeys is not null && DateTime.UtcNow < _cacheExpiry)
                return _cachedKeys;

            var json = await httpClient.GetStringAsync(certsUri);
            var certs = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;

            _cachedKeys = certs.Values.Select(pem =>
            {
                var cert = X509Certificate2.CreateFromPem(pem);
                return (SecurityKey)new X509SecurityKey(cert);
            }).ToList();

            _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
            return _cachedKeys;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
