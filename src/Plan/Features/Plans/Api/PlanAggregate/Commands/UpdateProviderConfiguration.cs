namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class UpdateProviderConfiguration : IFeatureModule
{
    public record Request(
        string ExternalProductId,
        string ExternalPriceId
    );

    public static Func<IService, Guid, string, Request, Task<IResult>> Handler =>
        async (service, id, provider, request) =>
        {
            await service.HandleAsync(id, provider, request);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/plans/{id}/provider-configurations/{provider}", Handler);
    }

    public interface IService
    {
        Task HandleAsync(Guid id, string provider, Request request);
    }

    [Injectable]
    public class Service(
        Plan.UpdateProviderConfiguration updateProviderConfig,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, string provider, Request request)
        {
            var plan = await repository.Get(id);

            var command = new UpdateProviderConfigurationCommand(
                provider,
                request.ExternalProductId,
                request.ExternalPriceId
            );

            updateProviderConfig.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Plan, Guid> { }
}
