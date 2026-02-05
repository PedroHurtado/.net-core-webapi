namespace Menus.Features.Menus.Api.MenuAggregate.Commands;

public class UpdateMenuCategory : IFeatureModule
{
    public record Request(
        string Name,
        string? Description,
        int DisplayOrder
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/menus/{id}/categories/{categoryId}", Handler);
    }

    public static Func<IService, Guid, Guid, Request, Task<IResult>> Handler => async (service, id, categoryId, request) =>
    {
        await service.HandleAsync(id, categoryId, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, Guid categoryId, Request request);
    }

    [Injectable]
    public class Service(
        Menu.UpdateCategory updateCategory,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Guid categoryId, Request request)
        {
            var menu = await repository.Get(id);

            var command = new UpdateCategoryCommand(
                CategoryId: categoryId,
                Name: request.Name,
                Description: request.Description,
                DisplayOrder: request.DisplayOrder
            );

            updateCategory.Execute(menu, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    [Include<Menu>("Categories")]
    public interface IRepository : IUpdate<Menu, Guid> { }
}
