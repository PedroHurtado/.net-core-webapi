namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class UpdatePlanPricingTier : IFeatureModule
{
    public record Request(
        decimal Amount,
        string CurrencyCode
    );

    public static Func<IService, Guid, BillingPeriod, Request, Task<IResult>> Handler =>
        async (service, id, billingPeriod, request) =>
        {
            var response = await service.HandleAsync(id, billingPeriod, request);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/plans/{id}/pricing-tiers/{billingPeriod}", Handler);
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Guid id, BillingPeriod billingPeriod, Request request);
    }

    [Injectable]
    public class Service(
        Plan.UpdatePricingTier updatePricingTier,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Guid id, BillingPeriod billingPeriod, Request request)
        {
            var plan = await repository.Get(id);

            var command = new UpdatePricingTierCommand(
                billingPeriod,
                request.Amount,
                request.CurrencyCode
            );

            updatePricingTier.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();

            return PlanResponse.Map(plan);
        }
    }

    [Include<Plan>("Features", "PricingTiers")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}
