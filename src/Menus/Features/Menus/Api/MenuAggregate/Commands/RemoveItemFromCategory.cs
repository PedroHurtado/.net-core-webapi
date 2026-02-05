namespace Menus.Features.Menus.Api.MenuAggregate.Commands;

public class RemoveItemFromCategory : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/menus/{id}/categories/{categoryId}/items/{menuItemId}", Handler);
    }

    public static Func<IService, Guid, Guid, Guid, Task<IResult>> Handler => async (service, id, categoryId, menuItemId) =>
    {
        await service.HandleAsync(id, categoryId, menuItemId);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, Guid categoryId, Guid menuItemId);
    }

    [Injectable]
    public class Service(
        Menu.RemoveItemFromCategory removeItemFromCategory,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Guid categoryId, Guid menuItemId)
        {
            var menu = await repository.Get(id);

            var command = new RemoveItemFromCategoryCommand(
                CategoryId: categoryId,
                MenuItemId: menuItemId
            );

            removeItemFromCategory.Execute(menu, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    [Include<Menu>("Categories.Items.MenuItem")]
    public interface IRepository : IUpdate<Menu, Guid> { }
}
