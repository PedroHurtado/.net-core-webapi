namespace Menus.Features.Menus.Api.MenuItemAggregate.Commands;

public class UpdateMenuItemPriceOption : IFeatureModule
{
    public record Request(
        decimal? Price,
        bool IsActive
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/menu-items/{id}/price-options/{portionType}", Handler);
    }

    public static Func<IService, Guid, PortionType, Request, Task<IResult>> Handler => async (service, id, portionType, request) =>
    {
        await service.HandleAsync(id, portionType, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, PortionType portionType, Request request);
    }

    [Injectable]
    public class Service(
        MenuItem.UpdatePriceOption updatePriceOption,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, PortionType portionType, Request request)
        {
            var menuItem = await repository.Get(id);

            var command = new UpdatePriceOptionCommand(
                PortionType: portionType,
                Price: request.Price,
                IsActive: request.IsActive
            );

            updatePriceOption.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
