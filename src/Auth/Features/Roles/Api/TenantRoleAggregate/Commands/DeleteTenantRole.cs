namespace Auth.Features.Roles.Api.TenantRoleAggregate.Commands;

public class DeleteTenantRole : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            await service.HandleAsync(id);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/tenant-roles/{id}", Handler)
            .WithDescriptionCatalog("Delete tenant role");
    }

    public interface IService
    {
        Task HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id)
        {
            var role = await repository.Get(id);

            ConflictGuard.ThrowIf(role.IsOwner, "This role cannot be deleted");

            repository.Remove(role);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IRemove<TenantRole, Guid> { }
}
