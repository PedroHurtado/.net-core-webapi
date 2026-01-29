namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class DeactivateProviderConfiguration : IFeatureModule
{
    public static Func<IService, Guid, string, Task<IResult>> Handler =>
        async (service, id, provider) =>
        {
            var response = await service.HandleAsync(id, provider);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/plans/{id}/provider-configurations/{provider}/deactivate", Handler);
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Guid id, string provider);
    }

    [Injectable]
    public class Service(
        Plan.DeactivateProviderConfiguration deactivateProviderConfig,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Guid id, string provider)
        {
            var plan = await repository.Get(id);

            var command = new DeactivateProviderConfigurationCommand(provider);
            deactivateProviderConfig.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();

            return PlanResponse.Map(plan);
        }
    }

    public interface IRepository : IUpdate<Plan, Guid> { }
}
