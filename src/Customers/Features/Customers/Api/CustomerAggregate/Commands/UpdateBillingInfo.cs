namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateBillingInfo : IFeatureModule
{
    public record Request(
        string BusinessName,
        string TaxId,
        AddressRequest BillingAddress);

    public record AddressRequest(
        string Street,
        string City,
        string PostalCode,
        string Region,
        string Country,
        decimal Latitude,
        decimal Longitude);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/customer/billing-info", Handler);
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
        Customer.UpdateBillingInfo updateBillingInfo,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Request request)
        {
            var customer = await repository.Get(tenantId);

            var command = new UpdateBillingInfoCommand(
                BusinessName: request.BusinessName,
                TaxId: request.TaxId,
                BillingAddress: new CreateAddressCommand(
                    request.BillingAddress.Street,
                    request.BillingAddress.City,
                    request.BillingAddress.PostalCode,
                    request.BillingAddress.Region,
                    request.BillingAddress.Country,
                    request.BillingAddress.Latitude,
                    request.BillingAddress.Longitude));

            updateBillingInfo.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
