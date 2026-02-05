namespace Menus.Features.Menus.Api.AllergenAggregate.Commands;

public class CreateAllergen : IFeatureModule
{
    public record Request(
        string Code,
        string Name,
        string? IconUrl = null,
        bool IsActive = true,
        int DisplayOrder = 0
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
        app.MapPost("/allergens", Handler);
    }

    public static Func<IService, Request, Task<IResult>> Handler => async (service, request) =>
    {
        var response = await service.HandleAsync(request);
        return Results.Created($"/allergens/{response.Id}", response);
    };

    public interface IService
    {
        Task<Response> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Allergen.Create createAllergen,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<Response> HandleAsync(Request request)
        {
            var command = new CreateAllergenCommand(
                request.Code,
                request.Name,
                request.IconUrl,
                request.IsActive,
                request.DisplayOrder
            );

            var allergen = createAllergen.Execute(command);

            repository.Add(allergen);
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

    public interface IRepository : IAdd<Allergen> { }
}
