namespace Customer.Features.Menus.Api.MenuItemAggregate.Commands;

public class AddMenuItemPriceOption : IFeatureModule
{
    public record Request(
        PortionType PortionType,
        decimal? Price,
        bool IsActive = true
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menu-items/{id}/price-options", Handler);
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
        MenuItem.AddPriceOption addPriceOption,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var menuItem = await repository.Get(id);

            var command = new AddPriceOptionCommand(
                PortionType: request.PortionType,
                Price: request.Price,
                IsActive: request.IsActive
            );

            addPriceOption.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
