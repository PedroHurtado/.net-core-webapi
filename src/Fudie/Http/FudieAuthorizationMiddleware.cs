using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using Fudie.Features;
using Fudie.Security;

namespace Fudie.Http;

public class FudieAuthorizationMiddleware(RequestDelegate next)
{
    private const string InternalKeyHeader = "X-Internal-Key";

    public async Task InvokeAsync(
        HttpContext context,
        IJwtValidator jwtValidator,
        IConfiguration configuration)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            await next(context);
            return;
        }

        var metadata = endpoint.Metadata;

        if (metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await TrySetTokenContext(context, jwtValidator);
            await next(context);
            return;
        }

        if (metadata.GetMetadata<InternalRequirement>() is not null)
        {
            var internalSecret = configuration["Fudie:InternalSecret"];
            var incomingKey = context.Request.Headers[InternalKeyHeader].FirstOrDefault();

            if (string.IsNullOrEmpty(incomingKey) || incomingKey != internalSecret)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await TrySetTokenContext(context, jwtValidator);
            await next(context);
            return;
        }

        var tokenContext = await ValidateJwt(context, jwtValidator);
        if (tokenContext is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (metadata.GetMetadata<PlatformRequirement>() is not null)
        {
            if (tokenContext.TenantId is not null)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        var groupRequirement = metadata.GetMetadata<GroupRequirement>();
        if (groupRequirement is not null)
        {
            if (!tokenContext.Groups.Contains(groupRequirement.Group))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        SetTokenContext(context, tokenContext);
        await next(context);
    }

    private static async Task<FudieTokenContext?> ValidateJwt(HttpContext context, IJwtValidator jwtValidator)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        return await jwtValidator.ValidateTokenAsync(authHeader["Bearer ".Length..]);
    }

    private static async Task TrySetTokenContext(HttpContext context, IJwtValidator jwtValidator)
    {
        var tokenContext = await ValidateJwt(context, jwtValidator);
        SetTokenContext(context, tokenContext);
    }

    private static void SetTokenContext(HttpContext context, FudieTokenContext? tokenContext)
    {
        if (tokenContext is not null)
            context.Items["FudieTokenContext"] = tokenContext;
    }
}
