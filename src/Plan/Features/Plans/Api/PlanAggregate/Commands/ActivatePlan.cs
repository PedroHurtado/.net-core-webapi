namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class ActivatePlan : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/plans/{id}/activate", Handler);
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        Plan.Activate planActivate,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Guid id)
        {
            var plan = await repository.Get(id);

            planActivate.Execute(plan, new ActivatePlanCommand());

            await unitOfWork.SaveChangesAsync();

            return PlanResponse.Map(plan);
        }
    }

    [Include<Plan>("Features", "PricingTiers")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}
