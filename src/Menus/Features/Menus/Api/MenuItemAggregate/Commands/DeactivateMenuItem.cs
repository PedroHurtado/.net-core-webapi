namespace Menus.Features.Menus.Api.MenuItemAggregate.Commands;

public class DeactivateMenuItem : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menu-items/{id}/deactivate", Handler);
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
        MenuItem.Deactivate deactivateMenuItem,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MenuItemResponse> HandleAsync(Guid id)
        {
            var menuItem = await repository.Get(id);

            var command = new DeactivateMenuItemCommand();

            deactivateMenuItem.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();

            return MenuItemResponse.Map(menuItem);
        }
    }

    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
