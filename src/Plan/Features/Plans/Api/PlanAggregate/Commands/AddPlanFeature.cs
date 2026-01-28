namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class AddPlanFeature : IFeatureModule
{
    public record Request(
        string Code,
        string Name,
        string? Description,
        FeatureType Type,
        int? Limit,
        string? Unit
    );

    public static Func<IService, Guid, Request, Task<IResult>> Handler =>
        async (service, id, request) =>
        {
            var response = await service.HandleAsync(id, request);
            return Results.Created($"/plans/{response.Id}", response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/plans/{id}/features", Handler);
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        Plan.AddFeature addFeature,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Guid id, Request request)
        {
            var plan = await repository.Get(id);

            var command = new AddFeatureCommand(
                request.Code,
                request.Name,
                request.Description,
                request.Type,
                request.Limit,
                request.Unit
            );

            addFeature.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();

            return PlanResponse.Map(plan);
        }
    }

    [Include<Plan>("Features", "ProviderConfigurations")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}