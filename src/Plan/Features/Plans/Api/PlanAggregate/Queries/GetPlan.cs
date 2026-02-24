namespace Plans.Features.Plans.Api.PlanAggregate.Queries;

public class GetPlan : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/plans/{id}", Handler)
            .AllowAnonymous()
            .WithDescriptionCatalog("Get plan by id");
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(IRepository repository) : IService
    {
        public async Task<PlanResponse> HandleAsync(Guid id)
        {
            var plan = await repository.Get(id);
            return PlanResponse.Map(plan);
        }
    }

    [AsNoTracking]
    public interface IRepository : IGet<Plan, Guid> { }
}
