namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateSocialLink : IFeatureModule
{
    public record Request(string Url);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/customer/social-links/{platform}", Handler)
            .WithDescriptionCatalog("Update social link");
    }

    public static Func<IService, string, Request, Task<IResult>> Handler => async (service, platform, request) =>
    {
        await service.HandleAsync(platform, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(string platform, Request request);
    }

    [Injectable]
    public class Service(
        Customer.UpdateSocialLink updateSocialLink,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(string platform, Request request)
        {
            var customer = await repository.Get(tenantId);

            var command = new UpdateSocialLinkCommand(
                Platform: platform,
                Url: request.Url);

            updateSocialLink.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
