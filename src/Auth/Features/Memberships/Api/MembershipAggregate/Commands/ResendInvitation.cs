namespace Auth.Features.Memberships.Api.MembershipAggregate.Commands;

public class ResendInvitation : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            await service.HandleAsync(id);
            return Results.Ok();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/memberships/{id}/resend-invitation", Handler);
    }

    public interface IService
    {
        Task HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        Membership.ResendInvitation resendInvitation,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);

            resendInvitation.Execute(entity);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Membership, Guid> { }
}
