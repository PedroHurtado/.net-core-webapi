namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateAddress : IFeatureModule
{
    public record Request(
        string Street,
        string City,
        string PostalCode,
        string Region,
        string Country,
        decimal Latitude,
        decimal Longitude);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/customer/address", Handler)
            .WithDescriptionCatalog("Update customer address");
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
        Customer.UpdateAddress updateAddress,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Request request)
        {
            var customer = await repository.Get(tenantId);

            var command = new UpdateAddressCommand(
                Street: request.Street,
                City: request.City,
                PostalCode: request.PostalCode,
                Region: request.Region,
                Country: request.Country,
                Latitude: request.Latitude,
                Longitude: request.Longitude);

            updateAddress.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
