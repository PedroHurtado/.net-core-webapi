namespace Auth.Features.Memberships.Api.MembershipAggregate.Commands;

public class CancelInvitation : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/memberships/{id}/cancel-invitation", Handler)
            .WithDescriptionCatalog("Cancel membership invitation");
    }

    public interface IService
    {
        Task<MembershipResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        Membership.CancelInvitation cancelInvitation,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MembershipResponse> HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);

            cancelInvitation.Execute(entity);

            await unitOfWork.SaveChangesAsync();

            return MembershipResponse.Map(entity);
        }
    }

    [Include<Membership>("User", "Role")]
    public interface IRepository : IUpdate<Membership, Guid> { }
}
