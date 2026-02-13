namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class ActivatePlanPricingTier : IFeatureModule
{
    public static Func<IService, Guid, BillingPeriod, Task<IResult>> Handler =>
        async (service, id, billingPeriod) =>
        {
            var response = await service.HandleAsync(id, billingPeriod);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/plans/{id}/pricing-tiers/{billingPeriod}/activate", Handler);
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Guid id, BillingPeriod billingPeriod);
    }

    [Injectable]
    public class Service(
        Plan.ActivatePricingTier activatePricingTier,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Guid id, BillingPeriod billingPeriod)
        {
            var plan = await repository.Get(id);

            var command = new ActivatePricingTierCommand(billingPeriod);

            activatePricingTier.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();

            return PlanResponse.Map(plan);
        }
    }

    [Include<Plan>("Features", "PricingTiers")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}
