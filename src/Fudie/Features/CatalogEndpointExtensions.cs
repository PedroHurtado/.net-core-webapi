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

            var groups = new Dictionary<string, List<string>>();

            var readers = entries
                .Where(e => e.HttpVerb == "GET")
                .Select(e => e.ClassName)
                .ToList();

            if (readers.Count > 0)
                groups[$"{serviceId}:read"] = readers;

            var writers = entries
                .Where(e => e.HttpVerb != "GET")
                .Select(e => e.ClassName)
                .ToList();

            if (writers.Count > 0)
                groups[$"{serviceId}:write"] = writers;

            foreach (var custom in entries
                .Where(e => e.CustomGroup != null)
                .GroupBy(e => e.CustomGroup!))
            {
                groups[custom.Key] = [.. custom.Select(e => e.ClassName)];
            }

            return Results.Ok(groups);
        })
        .ExcludeFromDescription();
    }
}