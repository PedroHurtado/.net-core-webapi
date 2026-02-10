namespace Auth.Features.Roles.Api.TenantRoleAggregate.Commands;

public class UpdateTenantRolePermissions : IFeatureModule
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
        app.MapPut("/tenant-roles/{id}/permissions", Handler);
    }

    public interface IService
    {
        Task HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        TenantRole.UpdatePermissions updatePermissions,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var role = await repository.Get(id);

            var command = new UpdatePermissionsCommand(
                Groups: request.Groups,
                AdditionalScopes: request.AdditionalScopes,
                ExcludedScopes: request.ExcludedScopes);

            updatePermissions.Execute(role, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<TenantRole, Guid> { }
}
