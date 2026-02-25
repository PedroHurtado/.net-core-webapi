namespace Auth.Features.Roles.Api.TenantRoleAggregate.Queries;

public class GetTenantRole : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tenant-roles/{id}", Handler)
            .WithDescriptionCatalog("Get tenant role by id");
    }

    public interface IService
    {
        Task<TenantRoleResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(IRepository repository) : IService
    {
        public async Task<TenantRoleResponse> HandleAsync(Guid id)
        {
            var role = await repository.Get(id);
            return TenantRoleResponse.Map(role);
        }
    }

    [AsNoTracking]
    public interface IRepository : IGet<TenantRole, Guid> { }
}
