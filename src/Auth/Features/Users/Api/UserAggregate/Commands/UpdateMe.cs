namespace Auth.Features.Users.Api.UserAggregate.Commands;

public class UpdateMe : IFeatureModule
{
    public record Request(string Name, string? Phone);

    public static Func<IService, Request, Task<IResult>> Handler =>
        async (service, request) =>
        {
            var response = await service.HandleAsync(request);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/auth/me", Handler)
            .RequireAuthenticated()
            .WithDescriptionCatalog("Update current user");
    }

    public interface IService
    {
        Task<UserResponse> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        CurrentUserId currentUserId,
        User.Update updateUser,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<UserResponse> HandleAsync(Request request)
        {
            var user = await repository.Get(currentUserId.Value);

            updateUser.Execute(user, new UpdateUserCommand(request.Name, request.Phone));

            await unitOfWork.SaveChangesAsync();

            return UserResponse.Map(user);
        }
    }

    public interface IRepository : IGet<User, Guid> { }
}
