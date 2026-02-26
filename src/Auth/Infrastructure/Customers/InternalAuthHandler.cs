namespace Auth.Infrastructure.Customers;

[Injectable(ServiceLifetime.Transient)]
public class InternalAuthHandler(IConfiguration configuration) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var internalSecret = configuration["Fudie:InternalSecret"]
            ?? throw new InvalidOperationException("Fudie:InternalSecret not configured");
        request.Headers.Add("X-Internal-Key", internalSecret);
        return await base.SendAsync(request, cancellationToken);
    }
}
