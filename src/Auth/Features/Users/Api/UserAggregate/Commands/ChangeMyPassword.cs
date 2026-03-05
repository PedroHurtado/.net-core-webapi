namespace Auth.Features.Users.Api.UserAggregate.Commands;

public class ChangeMyPassword : IFeatureModule
{
    public record Request(string CurrentPassword, string NewPassword);

    public static Func<IService, Request, Task<IResult>> Handler =>
        async (service, request) =>
        {
            await service.HandleAsync(request);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/me/change-password", Handler)
            .RequirePlatform()
            .WithDescriptionCatalog("Change password");
    }

    public interface IService
    {
        Task HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        CurrentUserId currentUserId,
        User.ChangePassword changePassword,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Request request)
        {
            var user = await repository.Get(currentUserId.Value);

            changePassword.Execute(user, new ChangePasswordCommand(
                request.CurrentPassword, request.NewPassword));

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IGet<User, Guid> { }
}
