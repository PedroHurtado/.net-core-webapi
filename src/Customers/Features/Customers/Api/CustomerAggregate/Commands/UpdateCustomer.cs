namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateCustomer : IFeatureModule
{
    public record Request(
        string Name,
        string Slug,
        string? Description,
        string? LogoUrl,
        string EstablishmentType,
        string DefaultCulture,
        string TimeZoneId,
        string[] CuisineTypes,
        string[] ServiceAmenities,
        string[] DietaryOptions);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/customer", Handler);
    }

    public static Func<IService, Request, Task<IResult>> Handler => async (service, request) =>
    {
        await service.HandleAsync(request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Customer.Update updateCustomer,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Request request)
        {
            var customer = await repository.Get(tenantId);

            if (customer.Slug != request.Slug)
            {
                ConflictGuard.ThrowIf(
                    await repository.ExistsBySlug(request.Slug),
                    $"Customer with slug '{request.Slug}' already exists");
            }

            var command = new UpdateCustomerCommand(
                Name: request.Name,
                Slug: request.Slug,
                Description: request.Description,
                LogoUrl: request.LogoUrl,
                EstablishmentType: request.EstablishmentType,
                DefaultCulture: request.DefaultCulture,
                TimeZoneId: request.TimeZoneId,
                CuisineTypes: request.CuisineTypes,
                ServiceAmenities: request.ServiceAmenities,
                DietaryOptions: request.DietaryOptions);

            updateCustomer.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Customer, Guid>
    {
        Task<bool> ExistsBySlug(string slug);
    }
}
