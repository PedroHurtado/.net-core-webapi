using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using Fudie.Features;

namespace Fudie.Http;

public class FudieAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public FudieAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICatalogRegistry catalog,
        IConfiguration configuration)
    {
        var endpoint = context.GetEndpoint();

        Console.WriteLine($"[Fudie Auth DEBUG] endpoint={endpoint?.DisplayName ?? "NULL"}, map_count={catalog.EndpointMapCount}");

        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var className = catalog.FindClassName(endpoint);

        Console.WriteLine($"[Fudie Auth DEBUG] className={className ?? "NULL"}, endpoint_hash={endpoint.GetHashCode()}");

        if (className != null)
        {
            var serviceId = configuration["Fudie:ServiceId"];
            var entry = catalog.GetAll().FirstOrDefault(e => e.ClassName == className);
            var scope = $"{serviceId}:{className}";

            Console.WriteLine(entry != null
                ? $"[Fudie Auth] {scope} -> found in catalog ({entry.HttpVerb})"
                : $"[Fudie Auth] {scope} -> NOT in catalog");
        }

        await _next(context);
    }
}
