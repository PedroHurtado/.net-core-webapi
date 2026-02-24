namespace Menus.Features.Menus.Api.MenuAggregate.Commands;

public class RemoveMenuDepositPolicy : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/menus/{id}/deposit-policy", Handler)
            .RequireGroup("menu:deposit")
            .WithDescriptionCatalog("Remove deposit policy from a menu");
    }

    public static Func<IService, Guid, Task<IResult>> Handler => async (service, id) =>
    {
        await service.HandleAsync(id);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        Menu.RemoveDepositPolicy removeDepositPolicy,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id)
        {
            var menu = await repository.Get(id);

            removeDepositPolicy.Execute(menu);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Menu, Guid> { }
}
