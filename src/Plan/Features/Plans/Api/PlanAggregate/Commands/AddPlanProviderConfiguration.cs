namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class AddPlanProviderConfiguration : IFeatureModule
{
    public record Request(
        string Provider,
        string ExternalProductId,
        string ExternalPriceId,
        bool IsActive = true
    );

    public static Func<IService, Guid, Request, Task<IResult>> Handler =>
        async (service, id, request) =>
        {
            var response = await service.HandleAsync(id, request);
            return Results.Created($"/plans/{response.Id}", response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/plans/{id}/provider-configurations", Handler);
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        Plan.AddProviderConfiguration addProviderConfig,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Guid id, Request request)
        {
            var plan = await repository.Get(id);

            var command = new AddProviderConfigurationCommand(
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

    [Include<Plan>("Features", "ProviderConfigurations")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}
