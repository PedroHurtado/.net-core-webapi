namespace Auth.Features.Roles.Api.TenantRoleAggregate.Commands;

public class UpdateTenantRole : IFeatureModule
{
    public record Request(string Name, string Description);

    public static Func<IService, Guid, Request, Task<IResult>> Handler =>
        async (service, id, request) =>
        {
            await service.HandleAsync(id, request);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tenant-roles/{id}", Handler)
            .WithDescriptionCatalog("Update tenant role");
    }

    public interface IService
    {
        Task HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        TenantRole.Update updateTenantRole,
        IRepository repository,
        IQuery query,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var role = await repository.Get(id);

            var duplicate = await query.Query<TenantRole>()
                .AnyAsync(x=>x.Name == request.Name
                    && x.Id != role.Id);
            ConflictGuard.ThrowIf(duplicate, "A role with this name already exists");

            var command = new UpdateTenantRoleCommand(
                Name: request.Name,
                Description: request.Description);

            updateTenantRole.Execute(role, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<TenantRole, Guid> { }
}
