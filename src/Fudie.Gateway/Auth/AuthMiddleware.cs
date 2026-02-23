namespace Fudie.Gateway.Auth;

public sealed class AuthMiddleware(
    RequestDelegate next,
    IOptions<AuthOptions> options,
    IAnonymousRouteRegistry registry)
{
    private readonly AuthOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        if (registry.IsAnonymous(context.Request.Method, context.Request.Path))
        {
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

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }

    private async Task HandleCookieAuth(HttpContext context, IAuthService authService, string sessionCookie)
    {
        var cookieHeader = $"{_options.CookieName}={sessionCookie}";

        var response = await CallAuthService(context, () => authService.ResolveAuth(cookieHeader));
        if (response is null) return;

        InjectDownstreamToken(context, response);
        ForwardSetCookie(context, response);

        await next(context);
    }

    private async Task HandleApiKeyAuth(HttpContext context, IAuthService authService, string apiKey)
    {
        var headers = new Dictionary<string, string> { [_options.ApiKeyHeader] = apiKey };

        var response = await CallAuthService(context, () => authService.ResolveApiKey(headers));
        if (response is null) return;

        InjectDownstreamToken(context, response);

        await next(context);
    }

    private static async Task<HttpResponseMessage?> CallAuthService(
        HttpContext context, Func<Task<HttpResponseMessage>> call)
    {
        HttpResponseMessage response;
        try
        {
            response = await call();
        }
        catch (HttpRequestException)
        {
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            return null;
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
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
}
