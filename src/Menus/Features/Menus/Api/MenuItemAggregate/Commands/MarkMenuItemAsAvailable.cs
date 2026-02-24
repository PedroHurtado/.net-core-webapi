namespace Menus.Features.Menus.Api.MenuItemAggregate.Commands;

public class MarkMenuItemAsAvailable : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menu-items/{id}/mark-available", Handler)
            .WithDescriptionCatalog("Mark menu item as available");
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
        MenuItem.MarkAsAvailable markAsAvailable,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MenuItemResponse> HandleAsync(Guid id)
        {
            var menuItem = await repository.Get(id);

            var command = new MarkMenuItemAsAvailableCommand();

            markAsAvailable.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();

            return MenuItemResponse.Map(menuItem);
        }
    }

    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
