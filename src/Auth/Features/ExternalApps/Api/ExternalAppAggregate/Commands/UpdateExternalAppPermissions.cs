namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class UpdateExternalAppPermissions : IFeatureModule
{
    public record Request(
        List<string> Groups,
        List<string> AdditionalScopes,
        List<string> ExcludedScopes);

    public static Func<IService, Guid, Request, Task<IResult>> Handler =>
        async (service, id, request) =>
        {
            await service.HandleAsync(id, request);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/external-apps/{id}/permissions", Handler);
    }

    public interface IService
    {
        Task HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        ExternalApp.UpdatePermissions updatePermissions,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var entity = await repository.Get(id);

            var command = new UpdateExternalAppPermissionsCommand(
                Groups: request.Groups,
                AdditionalScopes: request.AdditionalScopes,
                ExcludedScopes: request.ExcludedScopes);

            updatePermissions.Execute(entity, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<ExternalApp, Guid> { }
}
