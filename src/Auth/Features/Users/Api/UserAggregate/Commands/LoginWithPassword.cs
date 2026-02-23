namespace Auth.Features.Users.Api.UserAggregate.Commands;

public class LoginWithPassword : IFeatureModule
{
    public record Request(
        string Email,
        string Password);

    public static Func<IService, ISessionCookieService, HttpContext, Request, Task<IResult>> Handler =>
        async (service, sessionCookieService, httpContext, request) =>
        {
            var sessionId = await service.HandleAsync(request);

            sessionCookieService.Append(httpContext, sessionId);

            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", Handler).AllowAnonymous();
    }

    public interface IService
    {
        Task<Guid> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        User.RecordLogin recordLogin,
        Session.Create createSession,
        Session.Refresh refreshSession,
        Session.SetTenantContext setTenantContext,
        IRequestTimestamp requestTimestamp,
        IPasswordHasher passwordHasher,
        IMembershipLookup membershipLookup,
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<Guid> HandleAsync(Request request)
        {
            var user = await userRepository.FindFirstByEmailAndProvider(
                request.Email, AuthProvider.Local);

            UnauthorizedGuard.ThrowIf(user is null, "Invalid credentials");

            var isValid = passwordHasher.Verify(
                request.Password, user!.Password!.Hash, user.Password.Salt);

            UnauthorizedGuard.ThrowIf(!isValid, "Invalid credentials");
            UnauthorizedGuard.ThrowIf(!user.IsActive, "User is inactive");

            recordLogin.Execute(user, new RecordLoginCommand(requestTimestamp.UtcNow));

            var session = await sessionRepository.FindFirstByUserId(user.Id);

            if (session?.IsExpired == true)
            {
                sessionRepository.Remove(session);
                session = null;
            }

            if (session is null)
            {
                session = createSession.Execute(new CreateSessionCommand(
                    user.Id, requestTimestamp.UtcNow, requestTimestamp.ExpiresAt));

                var membership = await membershipLookup.FindFirstByUserId(user.Id);
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
    public interface IUserRepository
    {
        Task<User?> FindFirstByEmailAndProvider(string email, AuthProvider provider);
    }

    public interface ISessionRepository : IAdd<Session>, IRemove<Session, Guid>
    {
        [Tracking]
        Task<Session?> FindFirstByUserId(Guid userId);
    }
}
