namespace Menus.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemPriceOption : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/menu-items/{id}/price-options/{portionType}", Handler)
            .WithDescriptionCatalog("Remove price option from a menu item");
    }

    public static Func<IService, Guid, PortionType, Task<IResult>> Handler => async (service, id, portionType) =>
    {
        await service.HandleAsync(id, portionType);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, PortionType portionType);
    }

    [Injectable]
    public class Service(
        MenuItem.RemovePriceOption removePriceOption,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, PortionType portionType)
        {
            var menuItem = await repository.Get(id);

            var command = new RemovePriceOptionCommand(PortionType: portionType);

            removePriceOption.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
