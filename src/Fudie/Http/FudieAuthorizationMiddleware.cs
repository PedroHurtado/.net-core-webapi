using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using Fudie.Features;
using Fudie.OpenApi;
using Fudie.Security;

namespace Fudie.Http;

public class FudieAuthorizationMiddleware(RequestDelegate next)
{
    private const string InternalKeyHeader = "X-Internal-Key";

    public async Task InvokeAsync(
        HttpContext context,
        IJwtValidator jwtValidator,
        ICatalogRegistry catalogRegistry,
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
                await WriteProblem(context, StatusCodes.Status401Unauthorized,
                    "Unauthorized", "Invalid internal key",
                    "https://tools.ietf.org/html/rfc7235#section-3.1");
                return;
            }

            await TrySetTokenContext(context, jwtValidator);
            await next(context);
            return;
        }

        var tokenContext = await ValidateJwt(context, jwtValidator);
        if (tokenContext is null)
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized,
                "Unauthorized", "Authentication required",
                "https://tools.ietf.org/html/rfc7235#section-3.1");
            return;
        }

        if (metadata.GetMetadata<PlatformRequirement>() is not null)
        {
            var platformTenantId = configuration["Fudie:PlatformTenantId"];
            if (platformTenantId is null
                || tokenContext.TenantId?.ToString() != platformTenantId)
            {
                await WriteProblem(context, StatusCodes.Status403Forbidden,
                    "Forbidden", "Platform access required",
                    "https://tools.ietf.org/html/rfc7231#section-6.5.3");
                return;
            }
        }

        if (tokenContext.IsOwner)
        {
            SetTokenContext(context, tokenContext);
            await next(context);
            return;
        }

        var className = catalogRegistry.FindClassName(endpoint);

        if (className is not null && tokenContext.ExcludedScopes.Contains(className))
        {
            await WriteProblem(context, StatusCodes.Status403Forbidden,
                "Forbidden", "Access to this endpoint has been revoked",
                "https://tools.ietf.org/html/rfc7231#section-6.5.3");
            return;
        }

        if (className is not null && tokenContext.AdditionalScopes.Contains(className))
        {
            SetTokenContext(context, tokenContext);
            await next(context);
            return;
        }

        var groupRequirement = metadata.GetMetadata<GroupRequirement>();
        if (groupRequirement is not null)
        {
            if (!tokenContext.Groups.Contains(groupRequirement.Group))
            {
                await WriteProblem(context, StatusCodes.Status403Forbidden,
                    "Forbidden", "Insufficient permissions",
                    "https://tools.ietf.org/html/rfc7231#section-6.5.3");
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

    private static async Task WriteProblem(
        HttpContext context, int statusCode, string title, string detail, string type)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new CustomProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = context.Request.Path,
            Extensions = new Dictionary<string, object>
            {
                ["traceId"] = context.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
