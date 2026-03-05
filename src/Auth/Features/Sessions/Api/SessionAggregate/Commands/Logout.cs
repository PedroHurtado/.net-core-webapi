namespace Auth.Features.Sessions.Api.SessionAggregate.Commands;

public class Logout : IFeatureModule
{
    public static Func<IService, ISessionCookieService, HttpContext, Task<IResult>> Handler =>
        async (service, sessionCookieService, httpContext) =>
        {
            await service.HandleAsync();

            sessionCookieService.Delete(httpContext);

            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/logout", Handler)
            .RequireAuthenticated()
            .WithDescriptionCatalog("Logout");
    }

    public interface IService
    {
        Task HandleAsync();
    }

    [Injectable]
    public class Service(
        IFudieUser fudieUser,
        IRepository repository,
        IUnitOfWork unitOfWork
        ) : IService
    {
        public async Task HandleAsync()
        {
            var sessionId = fudieUser.SessionId;

            UnauthorizedGuard.ThrowIf(sessionId is null, "Session not found");

            var session = await repository.FindFirstById(sessionId!.Value);

            UnauthorizedGuard.ThrowIf(session is null, "Invalid session");

            repository.Remove(session!);

            await unitOfWork.SaveChangesAsync();
        }
    }

    //[GenerateRepository<Session>]
    public interface IRepository:IRemove<Session,Guid>
    {
        Task<Session?> FindFirstById(Guid id);
        
    }
}
