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
            IConfiguration configuration) =>
        {
            var response = new
            {
                ServiceId = configuration["Fudie:ServiceId"],
                ServiceName = configuration["Fudie:ServiceName"],
                Entries = catalog.GetAll()
            };

            return Results.Ok(response);
        })
        .RequireInternal();
    }
}
