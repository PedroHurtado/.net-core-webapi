namespace Auth.Features.Users.Api.UserAggregate.Queries;

public class SearchUserByEmail : IFeatureModule
{
    public static Func<IService, string, Task<IResult>> Handler =>
        async (service, email) =>
        {
            var response = await service.HandleAsync(email);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users", Handler)
            .RequirePlatform()
            .WithDescriptionCatalog("Search user by email");
    }

    public interface IService
    {
        Task<UserResponse?> HandleAsync(string email);
    }

    [Injectable]
    public class Service(
        IRepository repository) : IService
    {
        public async Task<UserResponse?> HandleAsync(string email)
        {
            var user = await repository.FindFirstByEmail(email);
            return user is null ? null : UserResponse.Map(user);
        }
    }

    [AsNoTracking]
    [GenerateRepository<User>]
    public interface IRepository
    {
        Task<User?> FindFirstByEmail(string email);
    }
}
