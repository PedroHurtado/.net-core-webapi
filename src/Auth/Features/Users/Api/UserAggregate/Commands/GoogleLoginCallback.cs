namespace Auth.Features.Users.Api.UserAggregate.Commands;

public class GoogleLoginCallback : IFeatureModule
{
    public static Func<IService, ISessionCookieService, IOAuthSettings, HttpContext, string, string, Task<IResult>> Handler =>
        async (service, sessionCookieService, oauthSettings, httpContext, code, state) =>
        {
            var settings = oauthSettings.Get();
            var cookieState = httpContext.Request.Cookies[settings.StateCookieName];

            if (cookieState is null || cookieState != state)
                return Results.Unauthorized();

            httpContext.Response.Cookies.Delete(settings.StateCookieName, new CookieOptions
            {
                Path = "/auth/login/google"
            });

            var sessionId = await service.HandleAsync(code);

            sessionCookieService.Append(httpContext, sessionId);

            return Results.Redirect("/");
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/login/google", Handler)
            .AllowAnonymous();
            
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
        Session.Refresh refreshSession,
        Session.SetTenantContext setTenantContext,
        IRequestTimestamp requestTimestamp,
        IMembershipLookup membershipLookup,
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
                UnauthorizedGuard.ThrowIf(!existingUser.IsActive, "User is inactive");

                updateFromOAuth.Execute(existingUser, new UpdateFromOAuthCommand(
                    Email: claims.Email,
                    Name: claims.Name,
                    AvatarUrl: claims.Picture
                ));
                userId = existingUser.Id;
            }

            var session = await sessionRepository.FindFirstByUserId(userId);

            if (session?.IsExpired == true)
            {
                sessionRepository.Remove(session);
                session = null;
            }

            if (session is null)
            {
                session = createSession.Execute(new CreateSessionCommand(
                    userId, requestTimestamp.UtcNow, requestTimestamp.ExpiresAt));

                var membership = await membershipLookup.FindFirstByUserId(userId);
                if (membership is not null)
                {
                    setTenantContext.Execute(session, new SetTenantContextCommand(
                        membership.TenantId,
                        membership.Role.Id,
                        [.. membership.Role.Groups],
                        [.. membership.Role.AdditionalScopes],
                        [.. membership.Role.ExcludedScopes],
                        membership.Role.IsOwner));
                }

                sessionRepository.Add(session);
            }
            else
            {
                refreshSession.Execute(session, new RefreshSessionCommand(
                    requestTimestamp.UtcNow, requestTimestamp.ExpiresAt));
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

    public interface ISessionRepository : IAdd<Session>, IRemove<Session, Guid>
    {
        [Tracking]
        Task<Session?> FindFirstByUserId(Guid userId);
    }
}
