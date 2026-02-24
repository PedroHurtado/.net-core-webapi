namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class RemovePlanFeature : IFeatureModule
{
    public static Func<IService, Guid, string, Task<IResult>> Handler =>
        async (service, id, code) =>
        {
            await service.HandleAsync(id, code);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/plans/{id}/features/{code}", Handler)
            .RequirePlatform()
            .WithDescriptionCatalog("Remove feature from a plan");
    }

    public interface IService
    {
        Task HandleAsync(Guid id, string code);
    }

    [Injectable]
    public class Service(
        Plan.RemoveFeature removeFeature,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, string code)
        {
            var plan = await repository.Get(id);

            var command = new RemoveFeatureCommand(code);

            removeFeature.Execute(plan, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    [Include<Plan>("Features")]
    public interface IRepository : IUpdate<Plan, Guid> { }
}