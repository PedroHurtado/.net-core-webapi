namespace Customer.Features.Menus.Api.MenuAggregate.Queries;

public class GetMenus : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/menus", Handler);
    }

    public static Func<IService, bool?, Task<IResult>> Handler => async (service, isActive) =>
    {
        var response = await service.HandleAsync(isActive);
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<List<MenuResponse>> HandleAsync(bool? isActive);
    }

    [Injectable]
    public class Service(IQuery query) : IService
    {
        public async Task<List<MenuResponse>> HandleAsync(bool? isActive)
        {
            var queryable = query.Query<Menu>().AsQueryable();

            if (isActive.HasValue)
            {
                queryable = queryable.Where(m => m.IsActive == isActive.Value);
            }

            var menus = await queryable
                .Include(m => m.Categories)
                    .ThenInclude(c => c.Items)
                        .ThenInclude(i => i.MenuItem)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Name)
                .ToListAsync();

            return [.. menus.Select(MenuResponse.Map)];
        }
    }
}
