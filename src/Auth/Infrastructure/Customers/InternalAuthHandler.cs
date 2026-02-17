namespace Auth.Infrastructure.Customers;

[Injectable(ServiceLifetime.Transient)]
public class InternalAuthHandler(
    IInternalTokenService tokenService,
    IConfiguration configuration) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(configuration["Fudie:PlatformTenantId"]!);
        var token = tokenService.GenerateTokenInternal(tenantId);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
