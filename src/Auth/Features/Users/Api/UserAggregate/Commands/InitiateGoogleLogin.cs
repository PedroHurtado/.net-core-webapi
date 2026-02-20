namespace Auth.Features.Users.Api.UserAggregate.Commands;

public class InitiateGoogleLogin : IFeatureModule
{
    public static Func<IService, IOAuthSettings, HttpContext, IResult> Handler =>
        (service, oauthSettings, httpContext) =>
        {
            var settings = oauthSettings.Get();
            var result = service.Handle();

            httpContext.Response.Cookies.Append(settings.StateCookieName, result.State, new CookieOptions
            {
                Path = "/auth/login/google",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromSeconds(settings.StateExpirationSeconds)
            });

            return Results.Redirect(result.Url);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login/google", Handler)
            .AllowAnonymous()
            .ExcludeFromDescription();
    }

    public interface IService
    {
        GoogleOAuthUrl Handle();
    }

    [Injectable]
    public class Service(IGoogleOAuthUrlBuilder urlBuilder) : IService
    {
        public GoogleOAuthUrl Handle()
        {
            return urlBuilder.Build();
        }
    }
}
