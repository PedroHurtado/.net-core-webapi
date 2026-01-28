namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class UpdatePlanFeature : IFeatureModule
{
    public record Request(
        string Name,
        string? Description,
        FeatureType Type,
        int? Limit,
        string? Unit
    );

    public static Func<IService, Guid, string, Request, Task<IResult>> Handler =>
        async (service, id, code, request) =>
        {
            await service.HandleAsync(id, code, request);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/plans/{id}/features/{code}", Handler);
    }

    public interface IService
    {
        Task HandleAsync(Guid id, string code, Request request);
    }

    [Injectable]
    public class Service(
        Plan.UpdateFeature updateFeature,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, string code, Request request)
        {
            var plan = await repository.Get(id);

            var command = new UpdateFeatureCommand(
                code,
                request.Name,
                request.Description,
                request.Type,
                request.Limit,
                request.Unit
            );

            updateFeature.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    [Include<Plan>("Features")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}