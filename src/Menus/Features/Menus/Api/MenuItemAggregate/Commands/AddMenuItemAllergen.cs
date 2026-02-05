namespace Menus.Features.Menus.Api.MenuItemAggregate.Commands;

public class AddMenuItemAllergen : IFeatureModule
{
    public record Request(
        string AllergenId
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menu-items/{id}/allergens", Handler);
    }

    public static Func<IService, Guid, Request, Task<IResult>> Handler => async (service, id, request) =>
    {
        var response = await service.HandleAsync(id, request);
        return Results.Created($"/menu-items/{id}", response);
    };

    public interface IService
    {
        Task<MenuItemResponse> HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        MenuItem.AddAllergen addAllergen,
        IRepository repository,
        IEntityLookup lockup,        
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MenuItemResponse> HandleAsync(Guid id, Request request)
        {
            var menuItem = await repository.Get(id);
            var allergen = await lockup.GetRequiredAsync<Allergen, string>(request.AllergenId);

            var command = new AddAllergenCommand(allergen);

            addAllergen.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();

            return MenuItemResponse.Map(menuItem);
        }
    }

    [Include<MenuItem>("Allergens")]
    public interface IRepository : IUpdate<MenuItem, Guid> { }

}
