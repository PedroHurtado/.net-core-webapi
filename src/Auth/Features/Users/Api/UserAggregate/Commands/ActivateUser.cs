namespace Auth.Features.Users.Api.UserAggregate.Commands;

public class ActivateUser : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            await service.HandleAsync(id);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{id:guid}/activate", Handler)
            .RequirePlatform()
            .WithDescriptionCatalog("Activate user");
    }

    public interface IService
    {
        Task HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        User.Activate activateUser,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id)
        {
            var user = await repository.Get(id);

            activateUser.Execute(user, new ActivateUserCommand());

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IGet<User, Guid> { }
}
