using Fudie.Gateway.Catalog;
using Fudie.OpenApi;

namespace Fudie.Gateway.Auth;

public sealed class AuthMiddleware(
    RequestDelegate next,
    IOptions<AuthOptions> options,
    ICatalogRouteRegistry registry,
    IHostEnvironment environment)
{
    private readonly AuthOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";

        if (!registry.IsKnownRoute(method, path))
        {
            await next(context);
            return;
        }

        if (registry.IsInternal(method, path))
        {
            if (!environment.IsDevelopment())
            {
                await WriteProblem(context, StatusCodes.Status404NotFound,
                    "Not Found", "The requested resource was not found",
                    "https://tools.ietf.org/html/rfc7231#section-6.5.4");
                return;
            }

            await next(context);
            return;
        }

        if (registry.IsAnonymous(method, path))
        {
            await TryResolveAuth(context, authService);
            await next(context);
            return;
        }

        var sessionCookie = context.Request.Cookies[_options.CookieName];
        if (sessionCookie is not null)
        {
            await HandleCookieAuth(context, authService, sessionCookie);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await HandleApiKeyAuth(context, authService, authHeader["Bearer ".Length..]);
            return;
        }

        await WriteProblem(context, StatusCodes.Status401Unauthorized,
            "Unauthorized", "Authentication required",
            "https://tools.ietf.org/html/rfc7235#section-3.1");
    }

    private async Task TryResolveAuth(HttpContext context, IAuthService authService)
    {
        var sessionCookie = context.Request.Cookies[_options.CookieName];
        if (sessionCookie is not null)
        {
            var cookieHeader = $"{_options.CookieName}={sessionCookie}";
            var response = await CallAuthService(context, () => authService.ResolveAuth(cookieHeader));
            if (response is not null)
            {
                InjectDownstreamToken(context, response);
                ForwardSetCookie(context, response);
            }

            CleanClientCredentials(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var headers = new Dictionary<string, string> { [_options.ApiKeyHeader] = authHeader["Bearer ".Length..] };
            var response = await CallAuthService(context, () => authService.ResolveApiKey(headers));
            if (response is not null)
                InjectDownstreamToken(context, response);

            CleanClientCredentials(context);
        }
    }

    private async Task HandleCookieAuth(HttpContext context, IAuthService authService, string sessionCookie)
    {
        var cookieHeader = $"{_options.CookieName}={sessionCookie}";

        var response = await CallAuthService(context, () => authService.ResolveAuth(cookieHeader));
        if (response is null) return;

        InjectDownstreamToken(context, response);
        ForwardSetCookie(context, response);
        CleanClientCredentials(context);

        await next(context);
    }

    private async Task HandleApiKeyAuth(HttpContext context, IAuthService authService, string apiKey)
    {
        var headers = new Dictionary<string, string> { [_options.ApiKeyHeader] = apiKey };

        var response = await CallAuthService(context, () => authService.ResolveApiKey(headers));
        if (response is null) return;

        InjectDownstreamToken(context, response);
        CleanClientCredentials(context);

        await next(context);
    }

    private async Task<HttpResponseMessage?> CallAuthService(
        HttpContext context, Func<Task<HttpResponseMessage>> call)
    {
        HttpResponseMessage response;
        try
        {
            response = await call();
        }
        catch (HttpRequestException)
        {
            await WriteProblem(context, StatusCodes.Status502BadGateway,
                "Bad Gateway", "Authentication service unavailable",
                "https://tools.ietf.org/html/rfc7231#section-6.6.3");
            return null;
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized)
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized,
                "Unauthorized", "Invalid credentials",
                "https://tools.ietf.org/html/rfc7235#section-3.1");
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            await WriteProblem(context, StatusCodes.Status502BadGateway,
                "Bad Gateway", "Authentication service error",
                "https://tools.ietf.org/html/rfc7231#section-6.6.3");
            return null;
        }

        return response;
    }

    private void InjectDownstreamToken(HttpContext context, HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues(_options.AuthTokenHeader, out var values))
        {
            context.Request.Headers.Authorization = $"Bearer {values.First()}";
        }
    }

    private void CleanClientCredentials(HttpContext context)
    {
        context.Request.Headers.Remove("Cookie");
        context.Request.Headers.Remove(_options.ApiKeyHeader);
    }

    private static void ForwardSetCookie(HttpContext context, HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            foreach (var cookie in cookies)
            {
                context.Response.Headers.Append("Set-Cookie", cookie);
            }
        }
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
