namespace Customer.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemNutritionalInfo : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/menu-items/{id}/nutritional-info", Handler);
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
        MenuItem.RemoveNutritionalInfo removeNutritionalInfo,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id)
        {
            var menuItem = await repository.Get(id);

            var command = new RemoveNutritionalInfoCommand();

            removeNutritionalInfo.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
