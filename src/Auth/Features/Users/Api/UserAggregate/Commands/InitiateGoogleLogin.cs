namespace Auth.Features.Users.Api.UserAggregate.Commands;

public class InitiateGoogleLogin : IFeatureModule
{
    public static Func<IService, HttpContext, IResult> Handler =>
        (service, httpContext) =>
        {
            var result = service.Handle();

            httpContext.Response.Cookies.Append("fudie_oauth_state", result.State, new CookieOptions
            {
                Path = "/auth/login/google",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromSeconds(300)
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
