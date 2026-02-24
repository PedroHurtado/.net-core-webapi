namespace Menus.Features.Menus.Api.MenuItemAggregate.Queries;

public class GetMenuItems : IFeatureModule
{
    public static Func<IService, bool?, bool?, Task<IResult>> Handler =>
        async (service, isActive, isAvailable) =>
        {
            var response = await service.HandleAsync(isActive, isAvailable);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/menu-items", Handler)
            .WithDescriptionCatalog("List all menu items");
    }

    public interface IService
    {
        Task<List<MenuItemResponse>> HandleAsync(bool? isActive, bool? isAvailable);
    }

    [Injectable]
    public class Service(IQuery query) : IService
    {
        public async Task<List<MenuItemResponse>> HandleAsync(bool? isActive, bool? isAvailable)
        {
            var queryable = query.Query<MenuItem>();

            if (isActive.HasValue)
            {
                queryable = queryable.Where(m => m.IsActive == isActive.Value);
            }

            if (isAvailable.HasValue)
            {
                queryable = queryable.Where(m => m.IsAvailable == isAvailable.Value);
            }

            var menuItems = await queryable
                .Include(m=>m.Allergens)
                .OrderBy(m => m.Name)
                .ToListAsync();

            return [.. menuItems.Select(MenuItemResponse.Map)];
        }
    }
}
