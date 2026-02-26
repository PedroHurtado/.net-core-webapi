namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class CreateCustomer : IFeatureModule
{
    public record Request(
        Guid Id,
        string Name,
        string Slug,
        string? Description,
        string EstablishmentType,
        string DefaultCulture,
        string TimeZoneId,
        CreateAddressRequest Address,
        CreateContactInfoRequest ContactInfo,
        CreateBillingInfoRequest BillingInfo);

    public record CreateAddressRequest(
        string Street,
        string City,
        string PostalCode,
        string Region,
        string Country,
        decimal Latitude,
        decimal Longitude);

    public record CreateContactInfoRequest(
        string Phone,
        string? Email,
        string? WebsiteUrl);

    public record CreateBillingInfoRequest(
        string BusinessName,
        string TaxId,
        CreateAddressRequest BillingAddress);

    public record Response(Guid Id, string Name);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/customer", Handler)
            .RequireInternal()
            .WithDescriptionCatalog("Create a new customer");
    }

    public static Func<IService, Request, Task<IResult>> Handler => async (service, request) =>
    {
        var response = await service.HandleAsync(request);
        return Results.Created("/customer", response);
    };

    public interface IService
    {
        Task<Response> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Customer.Create createCustomer,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<Response> HandleAsync(Request request)
        {
            ConflictGuard.ThrowIf(
                await repository.ExistsBySlug(request.Slug),
                $"Customer with slug '{request.Slug}' already exists");

            var command = new CreateCustomerCommand(
                Id: request.Id,
                Name: request.Name,
                Slug: request.Slug,
                Description: request.Description,
                EstablishmentType: request.EstablishmentType,
                DefaultCulture: request.DefaultCulture,
                TimeZoneId: request.TimeZoneId,
                Address: new CreateAddressCommand(
                    request.Address.Street,
                    request.Address.City,
                    request.Address.PostalCode,
                    request.Address.Region,
                    request.Address.Country,
                    request.Address.Latitude,
                    request.Address.Longitude),
                ContactInfo: new CreateContactInfoCommand(
                    request.ContactInfo.Phone,
                    request.ContactInfo.Email,
                    request.ContactInfo.WebsiteUrl),
                BillingInfo: new CreateBillingInfoCommand(
                    request.BillingInfo.BusinessName,
                    request.BillingInfo.TaxId,
                    new CreateAddressCommand(
                        request.BillingInfo.BillingAddress.Street,
                        request.BillingInfo.BillingAddress.City,
                        request.BillingInfo.BillingAddress.PostalCode,
                        request.BillingInfo.BillingAddress.Region,
                        request.BillingInfo.BillingAddress.Country,
                        request.BillingInfo.BillingAddress.Latitude,
                        request.BillingInfo.BillingAddress.Longitude)));

            var customer = createCustomer.Execute(command);

            repository.Add(customer);
            await unitOfWork.SaveChangesAsync();

            return new Response(customer.Id, customer.Name);
        }
    }

    public interface IRepository : IAdd<Customer>
    {
        Task<bool> ExistsBySlug(string slug);
    }
}
