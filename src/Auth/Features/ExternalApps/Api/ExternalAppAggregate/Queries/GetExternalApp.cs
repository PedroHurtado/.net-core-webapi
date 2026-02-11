namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate.Queries;

public class GetExternalApp : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/external-apps/{id}", Handler);
    }

    public interface IService
    {
        Task<ExternalAppResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(IRepository repository) : IService
    {
        public async Task<ExternalAppResponse> HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);
            return ExternalAppResponse.Map(entity);
        }
    }

    [AsNoTracking]
    [Include<ExternalApp>("User")]
    public interface IRepository : IGet<ExternalApp, Guid> { }
}
