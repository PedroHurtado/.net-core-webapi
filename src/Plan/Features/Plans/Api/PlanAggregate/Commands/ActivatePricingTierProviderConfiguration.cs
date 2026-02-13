namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class ActivatePricingTierProviderConfiguration : IFeatureModule
{
    public static Func<IService, Guid, BillingPeriod, string, Task<IResult>> Handler =>
        async (service, id, billingPeriod, provider) =>
        {
            var response = await service.HandleAsync(id, billingPeriod, provider);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/plans/{id}/pricing-tiers/{billingPeriod}/provider-configurations/{provider}/activate", Handler);
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Guid id, BillingPeriod billingPeriod, string provider);
    }

    [Injectable]
    public class Service(
        Plan.ActivatePricingTierProviderConfiguration activateProviderConfig,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Guid id, BillingPeriod billingPeriod, string provider)
        {
            var plan = await repository.Get(id);

            var command = new ActivatePricingTierProviderConfigurationCommand(
                billingPeriod,
                provider
            );

            activateProviderConfig.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();

            return PlanResponse.Map(plan);
        }
    }

    [Include<Plan>("Features", "PricingTiers")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}
