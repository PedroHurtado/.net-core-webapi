namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate.Queries;

public class GetExternalApps : IFeatureModule
{
    public static Func<IService, Task<IResult>> Handler =>
        async (service) =>
        {
            var response = await service.HandleAsync();
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/external-apps", Handler);
    }

    public interface IService
    {
        Task<List<ExternalAppResponse>> HandleAsync();
    }

    [Injectable]
    public class Service(IQuery query) : IService
    {
        public async Task<List<ExternalAppResponse>> HandleAsync()
        {
            var externalApps = await query.Query<ExternalApp>()
                .Include(x => x.User)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return [.. externalApps.Select(ExternalAppResponse.Map)];
        }
    }
}
