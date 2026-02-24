namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class UpdatePlan : IFeatureModule
{
    public record Request(
        string Name,
        string Description
    );

    public static Func<IService, Guid, Request, Task<IResult>> Handler =>
        async (service, id, request) =>
        {
            await service.HandleAsync(id, request);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/plans/{id}", Handler)
            .RequirePlatform()
            .WithDescriptionCatalog("Update plan details");
    }

    public interface IService
    {
        Task HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        Plan.Update planUpdate,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var plan = await repository.Get(id);

            var command = new UpdatePlanCommand(
                request.Name,
                request.Description
            );

            planUpdate.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Plan, Guid> { }
}
