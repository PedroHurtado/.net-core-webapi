namespace Auth.Features.Users.Api.UserAggregate.Queries;

public class GetUser : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:guid}", Handler)
            .RequirePlatform()
            .WithDescriptionCatalog("Get user by id");
    }

    public interface IService
    {
        Task<UserResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        IRepository repository) : IService
    {
        public async Task<UserResponse> HandleAsync(Guid id)
        {
            var user = await repository.Get(id);
            return UserResponse.Map(user);
        }
    }

    [AsNoTracking]
    public interface IRepository : IGet<User, Guid> { }
}
