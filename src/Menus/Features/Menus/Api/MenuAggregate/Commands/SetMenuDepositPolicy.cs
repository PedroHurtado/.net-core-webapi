namespace Menus.Features.Menus.Api.MenuAggregate.Commands;

public class SetMenuDepositPolicy : IFeatureModule
{
    public record Request(
        DepositType DepositType,
        decimal Amount,
        decimal? Percentage,
        decimal? MinimumBillForDeposit,
        int? MinimumGuestsForDeposit
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/menus/{id}/deposit-policy", Handler)
            .RequireGroup("menu:deposit")
            .WithDescriptionCatalog("Set deposit policy for a menu");
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
        Menu.SetDepositPolicy setDepositPolicy,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var menu = await repository.Get(id);

            var command = new SetDepositPolicyCommand(
                DepositType: request.DepositType,
                Amount: request.Amount,
                Percentage: request.Percentage,
                MinimumBillForDeposit: request.MinimumBillForDeposit,
                MinimumGuestsForDeposit: request.MinimumGuestsForDeposit
            );

            setDepositPolicy.Execute(menu, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Menu, Guid> { }
}
