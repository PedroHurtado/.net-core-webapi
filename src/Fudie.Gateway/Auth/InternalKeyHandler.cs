namespace Fudie.Gateway.Auth;

public sealed class InternalKeyHandler(IConfiguration configuration) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var internalSecret = configuration["Fudie:InternalSecret"]
            ?? throw new InvalidOperationException("Fudie:InternalSecret not configured");
        request.Headers.TryAddWithoutValidation("X-Internal-Key", internalSecret);
        return base.SendAsync(request, cancellationToken);
    }
}