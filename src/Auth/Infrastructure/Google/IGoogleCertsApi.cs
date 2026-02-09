namespace Auth.Infrastructure.Google;

public interface IGoogleCertsApi
{
    [Get("/oauth2/v1/certs")]
    Task<Dictionary<string, string>> GetCertificatesAsync();
}
