namespace Auth.Features.Sessions.Api.Commands;

public class ResolveAuth : IFeatureModule
{
    public static Func<IService, HttpContext, Task<IResult>> Handler =>
        async (service, httpContext) =>
        {
            var sessionId = httpContext.Request.Cookies["fudie_session"];

            UnauthorizedGuard.ThrowIf(sessionId is null, "Authentication required");

            var token = await service.HandleAsync(Guid.Parse(sessionId!));

            httpContext.Response.Cookies.Append("fudie_session", sessionId!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

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
        IInternalTokenService tokenService,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<string> HandleAsync(Guid sessionId)
        {
            var session = await repository.Get(sessionId);

            refreshSession.Execute(session);

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
