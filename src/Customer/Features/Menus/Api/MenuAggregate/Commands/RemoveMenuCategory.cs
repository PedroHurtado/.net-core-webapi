namespace Customer.Features.Menus.Api.MenuAggregate.Commands;

public class RemoveMenuCategory : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/menus/{id}/categories/{categoryId}", Handler);
    }

    public static Func<IService, Guid, Guid, Task<IResult>> Handler => async (service, id, categoryId) =>
    {
        await service.HandleAsync(id, categoryId);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, Guid categoryId);
    }

    [Injectable]
    public class Service(
        Menu.RemoveCategory removeCategory,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Guid categoryId)
        {
            var menu = await repository.Get(id);

            var command = new RemoveCategoryCommand(CategoryId: categoryId);

            removeCategory.Execute(menu, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    [Include<Menu>("Categories.Items")]
    public interface IRepository : IUpdate<Menu, Guid> { }
}
