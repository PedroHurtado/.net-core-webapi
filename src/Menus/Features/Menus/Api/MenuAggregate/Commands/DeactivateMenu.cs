namespace Menus.Features.Menus.Api.MenuAggregate.Commands;

public class DeactivateMenu : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menus/{id}/deactivate", Handler)
            .WithDescriptionCatalog("Deactivate a menu");
    }

    public static Func<IService, Guid, Task<IResult>> Handler => async (service, id) =>
    {
        var response = await service.HandleAsync(id);
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<MenuResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        Menu.Deactivate deactivateMenu,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MenuResponse> HandleAsync(Guid id)
        {
            var menu = await repository.Get(id);

            deactivateMenu.Execute(menu);

            await unitOfWork.SaveChangesAsync();

            return MenuResponse.Map(menu);
        }
    }

    [Include<Menu>("Categories.Items.MenuItem")]
    public interface IRepository : IUpdate<Menu, Guid> { }
}
