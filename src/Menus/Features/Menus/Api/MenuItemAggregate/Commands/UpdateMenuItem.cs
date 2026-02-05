namespace Menus.Features.Menus.Api.MenuItemAggregate.Commands;

public class UpdateMenuItem : IFeatureModule
{
    public record Request(
        string Name,
        string? Description,
        string? ImageUrl,
        int DisplayOrder,
        bool IsHighRiskItem,
        bool RequiresAdvanceOrder,
        int? MinimumAdvanceOrderQuantity,
        bool IsAlwaysAvailable,
        DayOfWeek[] AvailableDays,
        string? AllergenNotes
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/menu-items/{id}", Handler);
    }

    public static Func<IService, Guid, Request, Task<IResult>> Handler => async (service, id, request) =>
    {
        await service.HandleAsync(id, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        MenuItem.Update updateMenuItem,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var menuItem = await repository.Get(id);

            var command = new UpdateMenuItemCommand(
                Name: request.Name,
                Description: request.Description,
                ImageUrl: request.ImageUrl,
                DisplayOrder: request.DisplayOrder,
                IsHighRiskItem: request.IsHighRiskItem,
                RequiresAdvanceOrder: request.RequiresAdvanceOrder,
                MinimumAdvanceOrderQuantity: request.MinimumAdvanceOrderQuantity,
                IsAlwaysAvailable: request.IsAlwaysAvailable,
                AvailableDays: request.AvailableDays,
                AllergenNotes: request.AllergenNotes
            );

            updateMenuItem.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
