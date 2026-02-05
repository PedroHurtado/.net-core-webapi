namespace Menus.Features.Menus.Api.MenuAggregate.Commands;

public class AddMenuCategory : IFeatureModule
{
    public record Request(
        string Name,
        string? Description,
        int DisplayOrder = 0
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menus/{id}/categories", Handler);
    }

    public static Func<IService, Guid, Request, Task<IResult>> Handler => async (service, id, request) =>
    {
        var response = await service.HandleAsync(id, request);
        return Results.Created($"/menus/{response.Id}", response);
    };

    public interface IService
    {
        Task<MenuResponse> HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        Menu.AddCategory addCategory,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MenuResponse> HandleAsync(Guid id, Request request)
        {
            var menu = await repository.Get(id);

            var command = new AddCategoryCommand(
                Name: request.Name,
                Description: request.Description,
                DisplayOrder: request.DisplayOrder
            );

            addCategory.Execute(menu, command);

            await unitOfWork.SaveChangesAsync();

            return MenuResponse.Map(menu);
        }
    }

    [Include<Menu>("Categories.Items.MenuItem")]
    public interface IRepository : IUpdate<Menu, Guid> { }
}
