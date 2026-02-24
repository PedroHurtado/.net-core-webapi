namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class RemovePlanPricingTier : IFeatureModule
{
    public static Func<IService, Guid, BillingPeriod, Task<IResult>> Handler =>
        async (service, id, billingPeriod) =>
        {
            await service.HandleAsync(id, billingPeriod);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/plans/{id}/pricing-tiers/{billingPeriod}", Handler)
            .RequirePlatform()
            .WithDescriptionCatalog("Remove pricing tier from a plan");
    }

    public interface IService
    {
        Task HandleAsync(Guid id, BillingPeriod billingPeriod);
    }

    [Injectable]
    public class Service(
        Plan.RemovePricingTier removePricingTier,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, BillingPeriod billingPeriod)
        {
            var plan = await repository.Get(id);

            var command = new RemovePricingTierCommand(billingPeriod);

            removePricingTier.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    [Include<Plan>("Features", "PricingTiers")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}
