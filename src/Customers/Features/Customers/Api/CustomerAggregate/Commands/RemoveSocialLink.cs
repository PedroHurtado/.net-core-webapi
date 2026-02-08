namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class RemoveSocialLink : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/customer/social-links/{platform}", Handler);
    }

    public static Func<IService, string, Task<IResult>> Handler => async (service, platform) =>
    {
        await service.HandleAsync(platform);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(string platform);
    }

    [Injectable]
    public class Service(
        Customer.RemoveSocialLink removeSocialLink,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(string platform)
        {
            var customer = await repository.Get(tenantId);

            var command = new RemoveSocialLinkCommand(Platform: platform);
            removeSocialLink.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
