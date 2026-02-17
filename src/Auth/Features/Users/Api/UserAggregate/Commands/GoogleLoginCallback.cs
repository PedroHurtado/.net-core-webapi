namespace Auth.Features.Users.Api.UserAggregate.Commands;

public class GoogleLoginCallback : IFeatureModule
{
    public static Func<IService, HttpContext, string, string, Task<IResult>> Handler =>
        async (service, httpContext, code, state) =>
        {
            var cookieState = httpContext.Request.Cookies["fudie_oauth_state"];

            if (cookieState is null || cookieState != state)
                return Results.Unauthorized();

            httpContext.Response.Cookies.Delete("fudie_oauth_state", new CookieOptions
            {
                Path = "/auth/login/google"
            });

            var sessionId = await service.HandleAsync(code);

            httpContext.Response.Cookies.Append("fudie_session", sessionId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });

            return Results.Redirect("/");
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/login/google", Handler)
            .AllowAnonymous()
            .ExcludeFromDescription();
    }

    public interface IService
    {
        Task<Guid> HandleAsync(string code);
    }

    [Injectable]
    public class Service(
        IGoogleOAuthSettings googleOAuthSettings,
        IGoogleOAuthApi googleOAuthApi,
        IGoogleIdTokenValidator idTokenValidator,
        User.Create createUser,
        User.UpdateFromOAuth updateFromOAuth,
        Session.Create createSession,
        IRepository repository,
        ISessionRepository sessionRepository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<Guid> HandleAsync(string code)
        {
            var settings = googleOAuthSettings.Get();

            var tokenResponse = await googleOAuthApi.ExchangeCodeAsync(new GoogleTokenRequest(
                Code: code,
                ClientId: settings.ClientId,
                ClientSecret: settings.ClientSecret,
                RedirectUri: settings.RedirectUri
            ));

            var claims = await idTokenValidator.ValidateAsync(tokenResponse.IdToken);

            var providerId = $"google|{claims.Sub}";
            var existingUser = await repository.FindFirstByProviderIdAndProvider(providerId, AuthProvider.Google);

            Guid userId;

            if (existingUser is null)
            {
                var user = createUser.Execute(new CreateUserCommand(
                    ProviderId: providerId,
                    Provider: AuthProvider.Google,
                    Email: claims.Email,
                    Name: claims.Name,
                    AvatarUrl: claims.Picture
                ));

                repository.Add(user);
                userId = user.Id;
            }
            else
            {
                updateFromOAuth.Execute(existingUser, new UpdateFromOAuthCommand(
                    Email: claims.Email,
                    Name: claims.Name,
                    AvatarUrl: claims.Picture
                ));
                userId = existingUser.Id;
            }

            var session = await sessionRepository.FindFirstByUserId(userId);

            if (session is null)
            {
                session = createSession.Execute(new CreateSessionCommand(UserId: userId));
                sessionRepository.Add(session);
            }

            await unitOfWork.SaveChangesAsync();

            return session.Id;
        }
    }

    [GenerateRepository<User>]
    public interface IRepository : IAdd<User>
    {
        [Tracking]
        Task<User?> FindFirstByProviderIdAndProvider(string providerId, AuthProvider provider);
    }

    [GenerateRepository<Session>]
    public interface ISessionRepository : IAdd<Session>
    {
        Task<Session?> FindFirstByUserId(Guid userId);
    }
}
