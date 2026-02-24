namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class AddPricingTierProviderConfiguration : IFeatureModule
{
    public record Request(
        string Provider,
        string ExternalProductId,
        string ExternalPriceId,
        bool IsActive = true
    );

    public static Func<IService, Guid, BillingPeriod, Request, Task<IResult>> Handler =>
        async (service, id, billingPeriod, request) =>
        {
            var response = await service.HandleAsync(id, billingPeriod, request);
            return Results.Created($"/plans/{response.Id}", response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/plans/{id}/pricing-tiers/{billingPeriod}/provider-configurations", Handler)
            .RequirePlatform()
            .WithDescriptionCatalog("Add provider config to pricing tier");
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Guid id, BillingPeriod billingPeriod, Request request);
    }

    [Injectable]
    public class Service(
        Plan.AddPricingTierProviderConfiguration addProviderConfig,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Guid id, BillingPeriod billingPeriod, Request request)
        {
            var plan = await repository.Get(id);

            var command = new AddPricingTierProviderConfigurationCommand(
                billingPeriod,
                request.Provider,
                request.ExternalProductId,
                request.ExternalPriceId,
                request.IsActive
            );

            addProviderConfig.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();

            return PlanResponse.Map(plan);
        }
    }

    [Include<Plan>("Features", "PricingTiers")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}
