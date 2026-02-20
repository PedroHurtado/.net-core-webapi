using Microsoft.AspNetCore.Builder;

namespace Fudie.Http;

public static class FudieAuthorizationExtensions
{
    public static IApplicationBuilder UseFudieAuthorization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FudieAuthorizationMiddleware>();
    }
}
