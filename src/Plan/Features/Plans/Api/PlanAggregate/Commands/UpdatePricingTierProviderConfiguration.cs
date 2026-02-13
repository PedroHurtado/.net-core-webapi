namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class UpdatePricingTierProviderConfiguration : IFeatureModule
{
    public record Request(
        string ExternalProductId,
        string ExternalPriceId
    );

    public static Func<IService, Guid, BillingPeriod, string, Request, Task<IResult>> Handler =>
        async (service, id, billingPeriod, provider, request) =>
        {
            var response = await service.HandleAsync(id, billingPeriod, provider, request);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/plans/{id}/pricing-tiers/{billingPeriod}/provider-configurations/{provider}", Handler);
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Guid id, BillingPeriod billingPeriod, string provider, Request request);
    }

    [Injectable]
    public class Service(
        Plan.UpdatePricingTierProviderConfiguration updateProviderConfig,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Guid id, BillingPeriod billingPeriod, string provider, Request request)
        {
            var plan = await repository.Get(id);

            var command = new UpdatePricingTierProviderConfigurationCommand(
                billingPeriod,
                provider,
                request.ExternalProductId,
                request.ExternalPriceId
            );

            updateProviderConfig.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();

            return PlanResponse.Map(plan);
        }
    }

    [Include<Plan>("Features", "PricingTiers")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}
