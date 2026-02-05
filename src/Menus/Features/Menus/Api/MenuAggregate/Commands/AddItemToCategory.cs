namespace Menus.Features.Menus.Api.MenuAggregate.Commands;

public class AddItemToCategory : IFeatureModule
{
    public record Request(
        Guid MenuItemId,
        int DisplayOrder = 0,
        PriceOptionData[]? PriceOverrides = null
    );

    public record PriceOptionData(
        PortionType PortionType,
        decimal? Price,
        bool IsActive = true
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menus/{id}/categories/{categoryId}/items", Handler);
    }

    public static Func<IService, Guid, Guid, Request, Task<IResult>> Handler => async (service, id, categoryId, request) =>
    {
        var response = await service.HandleAsync(id, categoryId, request);
        return Results.Created($"/menus/{response.Id}", response);
    };

    public interface IService
    {
        Task<MenuResponse> HandleAsync(Guid id, Guid categoryId, Request request);
    }

    [Injectable]
    public class Service(
        Menu.AddItemToCategory addItemToCategory,
        IRepository repository,
        IEntityLookup entityLookup,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MenuResponse> HandleAsync(Guid id, Guid categoryId, Request request)
        {
            var menu = await repository.Get(id);
            var menuItem = await entityLookup.GetRequiredAsync<MenuItem, Guid>(request.MenuItemId);

            var command = new AddItemToCategoryCommand(
                CategoryId: categoryId,
                MenuItem: menuItem,
                DisplayOrder: request.DisplayOrder,
                PriceOverrides: request.PriceOverrides?
                    .Select(p => new Domain.MenuAggregate.PriceOptionData(p.PortionType, p.Price, p.IsActive))
                    .ToArray()
            );

            addItemToCategory.Execute(menu, command);

            await unitOfWork.SaveChangesAsync();

            return MenuResponse.Map(menu);
        }
    }

    [Include<Menu>("Categories.Items.MenuItem")]
    public interface IRepository : IUpdate<Menu, Guid> { }
}
