namespace Auth.Infrastructure.Google;

public interface IGoogleCertificateProvider
{
    Task<IList<SecurityKey>> GetSigningKeysAsync();
}

[Injectable]
public class GoogleCertificateProvider(IGoogleCertsApi certsApi) : IGoogleCertificateProvider
{
    private IList<SecurityKey>? _cachedKeys;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<IList<SecurityKey>> GetSigningKeysAsync()
    {
        if (_cachedKeys is not null && DateTime.UtcNow < _cacheExpiry)
            return _cachedKeys;

        await _semaphore.WaitAsync();
        try
        {
            if (_cachedKeys is not null && DateTime.UtcNow < _cacheExpiry)
                return _cachedKeys;

            var certs = await certsApi.GetCertificatesAsync();

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
