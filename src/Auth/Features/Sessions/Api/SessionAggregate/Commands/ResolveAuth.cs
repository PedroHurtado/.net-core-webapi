namespace Auth.Features.Sessions.Api.SessionAggregate.Commands;

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
        IMembershipLookup membershipLookup,
        IRepository repository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<string> HandleAsync(Guid sessionId)
        {
            var session = await repository.FindFirstById(sessionId);

            UnauthorizedGuard.ThrowIf(session is null, "Invalid session");

            try
            {
                refreshSession.Execute(session!, new RefreshSessionCommand(
                    requestTimestamp.UtcNow, requestTimestamp.ExpiresAt));
            }
            catch (UnauthorizedException)
            {
                await repository.DeleteById(session!.Id);
                throw;
            }

            var isUserActive = await userRepository.ExistsByIdAndIsActiveTrue(session!.UserId);
            UnauthorizedGuard.ThrowIf(!isUserActive, "User is inactive");

            if (session.HasTenantContext)
            {
                var isMembershipActive = await membershipLookup.ExistsByUserIdAndTenantId(
                    session.UserId, session.TenantId!.Value);
                UnauthorizedGuard.ThrowIf(!isMembershipActive, "Membership is inactive");
            }

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

    [GenerateRepository<Session>]
    public interface IRepository
    {
        [Tracking]
        Task<Session?> FindFirstById(Guid id);
        Task<int> DeleteById(Guid id);
    }

    [GenerateRepository<User>]
    public interface IUserRepository
    {
        Task<bool> ExistsByIdAndIsActiveTrue(Guid id);
    }
}
