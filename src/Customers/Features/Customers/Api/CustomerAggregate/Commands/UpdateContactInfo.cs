namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateContactInfo : IFeatureModule
{
    public record Request(
        string Phone,
        string? Email,
        string? WebsiteUrl);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/customer/contact-info", Handler);
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
        Customer.UpdateContactInfo updateContactInfo,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Request request)
        {
            var customer = await repository.Get(tenantId);

            var command = new UpdateContactInfoCommand(
                Phone: request.Phone,
                Email: request.Email,
                WebsiteUrl: request.WebsiteUrl);

            updateContactInfo.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
