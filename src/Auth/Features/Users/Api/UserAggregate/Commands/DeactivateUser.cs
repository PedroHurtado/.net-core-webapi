namespace Auth.Features.Users.Api.UserAggregate.Commands;

public class DeactivateUser : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            await service.HandleAsync(id);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{id:guid}/deactivate", Handler)
            .RequirePlatform()
            .WithDescriptionCatalog("Deactivate user");
    }

    public interface IService
    {
        Task HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        User.Deactivate deactivateUser,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id)
        {
            var user = await repository.Get(id);

            deactivateUser.Execute(user, new DeactivateUserCommand());

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IGet<User, Guid> { }
}
