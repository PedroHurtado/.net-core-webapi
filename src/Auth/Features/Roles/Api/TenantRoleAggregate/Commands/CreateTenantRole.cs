namespace Auth.Features.Roles.Api.TenantRoleAggregate.Commands;

public class CreateTenantRole : IFeatureModule
{
    public record Request(string Name, string Description);

    public static Func<IService, Request, Task<IResult>> Handler =>
        async (service, request) =>
        {
            var response = await service.HandleAsync(request);
            return Results.Created($"/tenant-roles/{response.Id}", response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/tenant-roles", Handler);
    }

    public interface IService
    {
        Task<TenantRoleResponse> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Guid tenantId,
        TenantRole.Create createTenantRole,
        IRepository repository,
        IQuery query,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<TenantRoleResponse> HandleAsync(Request request)
        {
            var duplicate = await query.Query<TenantRole>()
                .AnyAsync(x=>x.Name == request.Name);
            ConflictGuard.ThrowIf(duplicate, "A role with this name already exists");

            var command = new CreateTenantRoleCommand(
                TenantId: tenantId,
                Name: request.Name,
                Description: request.Description);

            var role = createTenantRole.Execute(command);

            repository.Add(role);
            await unitOfWork.SaveChangesAsync();

            return TenantRoleResponse.Map(role);
        }
    }

    public interface IRepository : IAdd<TenantRole> { }
}
