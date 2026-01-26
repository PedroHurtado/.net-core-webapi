namespace Customer.Features.Menus.Api.MenuItemAggregate.Commands;

public class SetMenuItemNutritionalInfo : IFeatureModule
{
    public record Request(
        int Calories,
        decimal Protein,
        decimal Carbohydrates,
        decimal Fat,
        int ServingSize,
        decimal? Fiber,
        decimal? Sugar,
        decimal? Salt
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/menu-items/{id}/nutritional-info", Handler);
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
        MenuItem.SetNutritionalInfo setNutritionalInfo,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var menuItem = await repository.Get(id);

            var command = new SetNutritionalInfoCommand(
                Calories: request.Calories,
                Protein: request.Protein,
                Carbohydrates: request.Carbohydrates,
                Fat: request.Fat,
                ServingSize: request.ServingSize,
                Fiber: request.Fiber,
                Sugar: request.Sugar,
                Salt: request.Salt
            );

            setNutritionalInfo.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
