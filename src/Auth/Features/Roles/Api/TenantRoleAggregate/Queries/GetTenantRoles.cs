namespace Auth.Features.Roles.Api.TenantRoleAggregate.Queries;

public class GetTenantRoles : IFeatureModule
{
    public static Func<IService, Task<IResult>> Handler =>
        async (service) =>
        {
            var response = await service.HandleAsync();
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tenant-roles", Handler);
    }

    public interface IService
    {
        Task<List<TenantRoleResponse>> HandleAsync();
    }

    [Injectable]
    public class Service(IQuery query) : IService
    {
        public async Task<List<TenantRoleResponse>> HandleAsync()
        {
            return await query.Query<TenantRole>()                
                .OrderBy(x => x.Name)
                .Select(x => new TenantRoleResponse(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.IsSystem,
                    x.IsEditable,
                    x.IsDeletable,
                    x.Groups,
                    x.AdditionalScopes,
                    x.ExcludedScopes))
                .ToListAsync();
        }
    }
}
