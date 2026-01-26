namespace Customer.Features.Menus.Api.MenuItemAggregate.Commands;

public class SetMenuItemDepositOverride : IFeatureModule
{
    public record Request(
        decimal DepositAmount,
        int? MinimumQuantityForDeposit
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/menu-items/{id}/deposit-override", Handler);
    }

    public static Func<IService, Guid, Request, Task<IResult>> Handler => async (service, id, request) =>
    {
        await service.HandleAsync(id, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        MenuItem.SetDepositOverride setDepositOverride,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var menuItem = await repository.Get(id);

            var command = new SetDepositOverrideCommand(
                DepositAmount: request.DepositAmount,
                MinimumQuantityForDeposit: request.MinimumQuantityForDeposit
            );

            setDepositOverride.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
