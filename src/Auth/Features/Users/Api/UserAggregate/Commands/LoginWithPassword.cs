namespace Auth.Features.Users.Api.UserAggregate.Commands;

public class LoginWithPassword : IFeatureModule
{
    public record Request(
        string Email,
        string Password);

    public static Func<IService, HttpContext, Request, Task<IResult>> Handler =>
        async (service, httpContext, request) =>
        {
            var sessionId = await service.HandleAsync(request);

            httpContext.Response.Cookies.Append("fudie_session", sessionId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", Handler);
    }

    public interface IService
    {
        Task<Guid> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        User.RecordLogin recordLogin,
        Session.Create createSession,
        IPasswordHasher passwordHasher,
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

            recordLogin.Execute(user);

            var session = createSession.Execute(new CreateSessionCommand(user.Id));

            sessionRepository.Add(session);
            await unitOfWork.SaveChangesAsync();

            return session.Id;
        }
    }

    [GenerateRepository<User>]
    public interface IUserRepository
    {
        Task<User?> FindFirstByEmailAndProvider(string email, AuthProvider provider);
    }

    public interface ISessionRepository : IAdd<Session> { }
}
