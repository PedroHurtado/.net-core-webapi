namespace Menus.Features.Menus.Api.MenuAggregate.Commands;

public class UpdateMenu : IFeatureModule
{
    public record Request(
        string Name,
        string? Description,
        DateTime? EffectiveFrom,
        DateTime? EffectiveUntil,
        int DisplayOrder
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/menus/{id}", Handler);
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
        Menu.Update updateMenu,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var menu = await repository.Get(id);

            var command = new UpdateMenuCommand(
                Name: request.Name,
                Description: request.Description,
                EffectiveFrom: request.EffectiveFrom,
                EffectiveUntil: request.EffectiveUntil,
                DisplayOrder: request.DisplayOrder
            );

            updateMenu.Execute(menu, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Menu, Guid> { }
}
