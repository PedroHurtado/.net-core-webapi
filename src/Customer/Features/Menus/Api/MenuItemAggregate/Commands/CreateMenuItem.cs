namespace Customer.Features.Menus.Api.MenuItemAggregate.Commands;

public class CreateMenuItem : IFeatureModule
{
    public record Request(
        string Name,
        string? Description,
        string? ImageUrl,
        int DisplayOrder,
        bool IsHighRiskItem,
        bool RequiresAdvanceOrder,
        int? MinimumAdvanceOrderQuantity,
        bool IsAlwaysAvailable,
        DayOfWeek[] AvailableDays,
        string? AllergenNotes,
        CreatePriceOptionRequest[] PriceOptions
    );

    public record CreatePriceOptionRequest(
        PortionType PortionType,
        decimal? Price,
        bool IsActive = true
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menu-items", Handler);
    }

    public static Func<IService, Request, Task<IResult>> Handler => async (service, request) =>
    {
        var response = await service.HandleAsync(request);
        return Results.Created($"/menu-items/{response.Id}", response);
    };

    public interface IService
    {
        Task<MenuItemResponse> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Guid tenantId,
        MenuItem.Create createMenuItem,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MenuItemResponse> HandleAsync(Request request)
        {
            var command = new CreateMenuItemCommand(
                TenantId: tenantId,
                Name: request.Name,
                Description: request.Description,
                ImageUrl: request.ImageUrl,
                DisplayOrder: request.DisplayOrder,
                IsHighRiskItem: request.IsHighRiskItem,
                RequiresAdvanceOrder: request.RequiresAdvanceOrder,
                MinimumAdvanceOrderQuantity: request.MinimumAdvanceOrderQuantity,
                IsAlwaysAvailable: request.IsAlwaysAvailable,
                AvailableDays: request.AvailableDays,
                AllergenNotes: request.AllergenNotes,
                PriceOptions: request.PriceOptions
                    .Select(po => new CreatePriceOptionCommand(po.PortionType, po.Price, po.IsActive))
                    .ToArray()
            );

            var menuItem = createMenuItem.Execute(command);

            repository.Add(menuItem);
            await unitOfWork.SaveChangesAsync();

            return MenuItemResponse.Map(menuItem);
        }
    }

    public interface IRepository : IAdd<MenuItem> { }
}
