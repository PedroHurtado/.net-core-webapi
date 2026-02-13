namespace Plans.Features.Plans.Api.PlanAggregate.Commands;

public class CreatePlan : IFeatureModule
{
    public record Request(
        string Name,
        string Description
    );

    public static Func<IService, Request, Task<IResult>> Handler =>
        async (service, request) =>
        {
            var response = await service.HandleAsync(request);
            return Results.Created($"/plans/{response.Id}", response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/plans", Handler);
    }

    public interface IService
    {
        Task<PlanResponse> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Plan.Create planCreate,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Request request)
        {
            var command = new CreatePlanCommand(
                request.Name,
                request.Description
            );

            var plan = planCreate.Execute(command);

            repository.Add(plan);
            await unitOfWork.SaveChangesAsync();

            return PlanResponse.Map(plan);
        }
    }

    public interface IRepository : IAdd<Plan> { }
}
