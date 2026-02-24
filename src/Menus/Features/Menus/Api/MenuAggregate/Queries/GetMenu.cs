namespace Menus.Features.Menus.Api.MenuAggregate.Queries;

public class GetMenu : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/menus/{id}", Handler)
            .WithDescriptionCatalog("Get menu by id");
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
    public class Service(IRepository repository) : IService
    {
        public async Task<MenuResponse> HandleAsync(Guid id)
        {
            var menu = await repository.Get(id);
            return MenuResponse.Map(menu);
        }
    }

    [AsNoTracking]
    [Include<Menu>("Categories.Items.MenuItem")]
    public interface IRepository : IGet<Menu, Guid> { }
}
