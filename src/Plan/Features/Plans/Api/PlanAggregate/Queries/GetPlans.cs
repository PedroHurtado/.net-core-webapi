namespace Plans.Features.Plans.Api.PlanAggregate.Queries;

public class GetPlans : IFeatureModule
{
    public static Func<IService, bool?, Task<IResult>> Handler =>
        async (service, isActive) =>
        {
            var response = await service.HandleAsync(isActive);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/plans", Handler);
    }

    public interface IService
    {
        Task<List<PlanResponse>> HandleAsync(bool? isActive);
    }

    [Injectable]
    public class Service(IQuery query) : IService
    {
        public async Task<List<PlanResponse>> HandleAsync(bool? isActive)
        {
            var queryable = query.Query<Plan>();

            if (isActive.HasValue)
            {
                queryable = queryable.Where(p => p.IsActive == isActive.Value);
            }

            var plans = await queryable
                .OrderBy(p => p.Name)
                .ToListAsync();

            return plans.Select(PlanResponse.Map).ToList();
        }
    }
}
