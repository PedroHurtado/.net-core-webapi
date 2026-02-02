namespace Customer.Features.Menus.Api.MenuAggregate.Commands;

public class CreateMenu : IFeatureModule
{
    public record Request(
        string Name,
        string? Description,
        DateTime? EffectiveFrom,
        DateTime? EffectiveUntil
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menus", Handler);
    }

    public static Func<IService, Request, Task<IResult>> Handler => async (service, request) =>
    {
        var response = await service.HandleAsync(request);
        return Results.Created($"/menus/{response.Id}", response);
    };

    public interface IService
    {
        Task<MenuResponse> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Guid tenantId,
        Menu.Create createMenu,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MenuResponse> HandleAsync(Request request)
        {
            var command = new CreateMenuCommand(
                TenantId: tenantId,
                Name: request.Name,
                Description: request.Description,
                EffectiveFrom: request.EffectiveFrom,
                EffectiveUntil: request.EffectiveUntil
            );

            var menu = createMenu.Execute(command);

            repository.Add(menu);
            await unitOfWork.SaveChangesAsync();

            return MenuResponse.Map(menu);
        }
    }

    public interface IRepository : IAdd<Menu> { }
}
