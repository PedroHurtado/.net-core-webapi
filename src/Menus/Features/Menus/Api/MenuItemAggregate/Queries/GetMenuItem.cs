namespace Menus.Features.Menus.Api.MenuItemAggregate.Queries;

public class GetMenuItem : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/menu-items/{id}", Handler)
            .WithDescriptionCatalog("Get menu item by id");
    }

    public static Func<IService, Guid, Task<IResult>> Handler => async (service, id) =>
    {
        var response = await service.HandleAsync(id);
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<MenuItemResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(IRepository repository) : IService
    {
        public async Task<MenuItemResponse> HandleAsync(Guid id)
        {
            var menuItem = await repository.Get(id);

            return MenuItemResponse.Map(menuItem);
        }
    }

    [AsNoTracking]
    [Include<MenuItem>("Allergens")]
    public interface IRepository : IGet<MenuItem, Guid> { }
}
