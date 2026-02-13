namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class AddPlanPricingTier : IFeatureModule
{
    public record Request(
        BillingPeriod BillingPeriod,
        decimal Amount,
        string CurrencyCode,
        bool IsActive = false
    );

    public static Func<IService, Guid, Request, Task<IResult>> Handler =>
        async (service, id, request) =>
        {
            var response = await service.HandleAsync(id, request);
            return Results.Created($"/plans/{response.Id}", response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/plans/{id}/pricing-tiers", Handler);
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        Plan.AddPricingTier addPricingTier,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Guid id, Request request)
        {
            var plan = await repository.Get(id);

            var command = new AddPricingTierCommand(
                request.BillingPeriod,
                request.Amount,
                request.CurrencyCode,
                request.IsActive
            );

            addPricingTier.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();

            return PlanResponse.Map(plan);
        }
    }

    [Include<Plan>("Features", "PricingTiers")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}
