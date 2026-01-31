namespace Customer.Features.Menus.Api.MenuAggregate.Commands;

public class UpdateCategoryItem : IFeatureModule
{
    public record Request(
        int DisplayOrder,
        PriceOptionData[]? PriceOverrides
    );

    public record PriceOptionData(
        PortionType PortionType,
        decimal? Price,
        bool IsActive = true
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/menus/{id}/categories/{categoryId}/items/{menuItemId}", Handler);
    }

    public static Func<IService, Guid, Guid, Guid, Request, Task<IResult>> Handler => async (service, id, categoryId, menuItemId, request) =>
    {
        await service.HandleAsync(id, categoryId, menuItemId, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, Guid categoryId, Guid menuItemId, Request request);
    }

    [Injectable]
    public class Service(
        Menu.UpdateCategoryItem updateCategoryItem,
        PriceOption.Create createPriceOption,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Guid categoryId, Guid menuItemId, Request request)
        {
            var menu = await repository.Get(id);

            var priceOverrides = request.PriceOverrides?
                .Select(p => createPriceOption.Execute(new CreatePriceOptionCommand(p.PortionType, p.Price, p.IsActive)))
                .ToHashSet();

            var command = new UpdateCategoryItemCommand(
                CategoryId: categoryId,
                MenuItemId: menuItemId,
                DisplayOrder: request.DisplayOrder,
                PriceOverrides: priceOverrides
            );

            updateCategoryItem.Execute(menu, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    [Include<Menu>("Categories.Items.MenuItem")]
    public interface IRepository : IUpdate<Menu, Guid> { }
}
