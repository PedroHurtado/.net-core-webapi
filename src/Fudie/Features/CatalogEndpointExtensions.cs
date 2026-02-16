using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Fudie.Features;

public static class CatalogEndpointExtensions
{
    public static void MapCatalog(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/catalog", (
            ICatalogRegistry catalog,
            IConfiguration configuration,
            ClaimsPrincipal user) =>
        {
            var serviceId = configuration["Fudie:ServiceId"];
            var platformTenantId = configuration["Fudie:PlatformTenantId"];
            var tid = user.FindFirst("tid")?.Value;

            var entries = tid == platformTenantId
                ? catalog.GetAll()
                : catalog.GetTenant();

            return Results.Ok(new CatalogResponse(serviceId!, entries));
        })
        .ExcludeFromDescription();
    }
}
