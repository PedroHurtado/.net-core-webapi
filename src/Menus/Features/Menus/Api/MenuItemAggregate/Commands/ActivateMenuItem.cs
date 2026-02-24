namespace Menus.Features.Menus.Api.MenuItemAggregate.Commands;

public class ActivateMenuItem : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menu-items/{id}/activate", Handler)
            .WithDescriptionCatalog("Activate a menu item");
    }

    public static Func<IService, Guid, Task<IResult>> Handler => async (service, id) =>
    {
        var response = await service.HandleAsync(id);
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<MenuItemResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        MenuItem.Activate activateMenuItem,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MenuItemResponse> HandleAsync(Guid id)
        {
            var menuItem = await repository.Get(id);

            var command = new ActivateMenuItemCommand();

            activateMenuItem.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();

            return MenuItemResponse.Map(menuItem);
        }
    }

    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
