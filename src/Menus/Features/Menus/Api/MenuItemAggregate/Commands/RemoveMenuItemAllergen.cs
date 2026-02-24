namespace Menus.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemAllergen : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/menu-items/{id}/allergens/{allergenId}", Handler)
            .WithDescriptionCatalog("Remove allergen from a menu item");
    }

    public static Func<IService, Guid, string, Task<IResult>> Handler => async (service, id, allergenId) =>
    {
        await service.HandleAsync(id, allergenId);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, string allergenId);
    }

    [Injectable]
    public class Service(
        MenuItem.RemoveAllergen removeAllergen,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, string allergenId)
        {
            var menuItem = await repository.Get(id);

            var command = new RemoveAllergenCommand(allergenId);
            removeAllergen.Execute(menuItem, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    [Include<MenuItem>("Allergens")]
    public interface IRepository : IUpdate<MenuItem, Guid> { }
}
