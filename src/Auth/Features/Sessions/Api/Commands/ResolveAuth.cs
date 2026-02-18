namespace Auth.Features.Sessions.Api.Commands;

public class ResolveAuth : IFeatureModule
{
    public static Func<IService, ISessionCookieService, ISessionSettings, HttpContext, Task<IResult>> Handler =>
        async (service, sessionCookieService, sessionSettings, httpContext) =>
        {
            var settings = sessionSettings.Get();
            var sessionId = httpContext.Request.Cookies[settings.CookieName];

            UnauthorizedGuard.ThrowIf(sessionId is null, "Authentication required");

            var token = await service.HandleAsync(Guid.Parse(sessionId!));

            sessionCookieService.Append(httpContext, Guid.Parse(sessionId!));

            httpContext.Response.Headers["X-Auth-Token"] = token;

            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/resolve", Handler)
            .AllowAnonymous()
            .ExcludeFromDescription();
    }

    public interface IService
    {
        Task<string> HandleAsync(Guid sessionId);
    }

    [Injectable]
    public class Service(
        Session.Refresh refreshSession,
        IRequestTimestamp requestTimestamp,
        IInternalTokenService tokenService,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<string> HandleAsync(Guid sessionId)
        {
            var session = await repository.Get(sessionId);

            refreshSession.Execute(session, new RefreshSessionCommand(
                requestTimestamp.UtcNow, requestTimestamp.ExpiresAt));

            await unitOfWork.SaveChangesAsync();

            return tokenService.GenerateSessionToken(new SessionTokenData(
                session.UserId,
                session.TenantId,
                session.IsOwner,
                session.Groups,
                session.AdditionalScopes,
                session.ExcludedScopes));
        }
    }
    public interface IRepository : IUpdate<Session, Guid> { }
}
