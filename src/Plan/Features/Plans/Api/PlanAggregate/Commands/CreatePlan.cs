namespace Plan.Features.Plans.Api.PlanAggregate.Commands;

public class CreatePlan : IFeatureModule
{
    public record Request(
        string Name,
        string Description,
        decimal Amount,
        string CurrencyCode,
        BillingPeriod BillingPeriod
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
        PlanAgg.Create planCreate,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<PlanResponse> HandleAsync(Request request)
        {
            var command = new CreatePlanCommand(
                request.Name,
                request.Description,
                request.Amount,
                request.CurrencyCode,
                request.BillingPeriod
            );

            var plan = planCreate.Execute(command);

            repository.Add(plan);
            await unitOfWork.SaveChangesAsync();

            return PlanResponse.Map(plan);
        }
    }

    public interface IRepository : IAdd<PlanAgg> { }
}