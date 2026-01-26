namespace Customer.Features.Menus.Api.AllergenAggregate.Commands;

public class UpdateAllergen : IFeatureModule
{
    public record Request(
        string Name,
        string? IconUrl,
        bool IsActive,
        int DisplayOrder
    );

    public record Response(
        string Id,
        string Name,
        string? IconUrl,
        bool IsActive,
        int DisplayOrder
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/allergens/{id}", Handler);
    }

    public static Func<IService, string, Request, Task<IResult>> Handler => async (service, id, request) =>
    {
        var response = await service.HandleAsync(id, request);
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<Response> HandleAsync(string id, Request request);
    }

    [Injectable]
    public class Service(
        Allergen.Update updateAllergen,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<Response> HandleAsync(string id, Request request)
        {
            var allergen = await repository.Get(id);

            var command = new UpdateAllergenCommand(
                request.Name,
                request.IconUrl,
                request.IsActive,
                request.DisplayOrder
            );

            updateAllergen.Execute(allergen, command);

            await unitOfWork.SaveChangesAsync();

            return new Response(
                allergen.Id,
                allergen.Name,
                allergen.IconUrl,
                allergen.IsActive,
                allergen.DisplayOrder
            );
        }
    }

    public interface IRepository : IUpdate<Allergen, string> { }
}
