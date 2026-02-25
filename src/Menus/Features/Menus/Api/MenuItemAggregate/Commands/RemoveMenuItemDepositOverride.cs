namespace Menus.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemDepositOverride : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/menu-items/{id}/deposit-override", Handler)
            .RequireGroup("menu:deposit", "Fianzas de menús")
            .WithDescriptionCatalog("Remove deposit override from a menu item");
    }

    public static Func<IService, Guid, Task<IResult>> Handler => async (service, id) =>
    {
        await service.HandleAsync(id);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        MenuItem.RemoveDepositOverride removeDepositOverride,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id)
        {
            var menuItem = await repository.Get(id);

            var command = new RemoveDepositOverrideCommand();

            removeDepositOverride.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
